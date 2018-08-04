using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ReviseApplication.Models;

namespace ReviseApplication.Hubs
{
    /* public class ChatHub : Hub
     {
         public void Send(string name, string message)
         {
             //Call the addNewMessageToPage method to update clients.
             Clients.All.addNewMessageToPage(name, message);    
         }
     }*/


    /// <summary>
    /// Hub Class which resposible to deal with event from the Client side and prompt functions in the Client side by by the Server side.
    /// </summary>
    [HubName("chatHub")]
    public class ChatHub : Hub
    {

        /// <summary>
        /// Class's attributes which store details and conections to the Data base.
        /// </summary>

        // DB Database class wich enable connection to the Revise Database.
        static ReviseDBEntities con = new ReviseDBEntities();

        //List which contains information about Users that connected to the hub.
        static List<ChatUserDetails> ConnectedUsers = new List<ChatUserDetails>();

        //List which contains messeages from the Revise DB.
        static List<message> CurrentMessages;

        //List which contains object of Roles - Project Members Roles.
        static List<role> rolesList;


        /// <summary>
        /// Function which responsible to deal with an User (Client) that connect to the Hub.
        /// </summary>
        public void join()
        {

            CurrentMessages = con.messages.ToList();

            var id = Context.ConnectionId;
            int catid = Convert.ToInt32(Clients.Caller.catid);
            int projid = Convert.ToInt32(Clients.Caller.projid);
            string userid = Clients.Caller.userid;

            rolesList = con.roles.ToList();
            //Query - Join Query which returns Records of Members Details and theirs Roles.

            var projectMembersDetails = from pm in con.projUsers.AsQueryable()
                                        join md in con.users.AsQueryable()
                                        on pm.user.userid equals md.userid
                                        where pm.project.ProjId == projid
                                        select new
                                        {
                                            userId = md.userid,
                                            userName = md.UserName.ToString(),
                                            firstName = md.fname,
                                            lastName = md.lname,
                                            picURL = md.pic,
                                            roleId = pm.role,
                                            roleName = pm.role1.RoleName,
                                        };

            var currentMemberDetails = projectMembersDetails.Where(m => m.userId == userid).First();
            var userName = currentMemberDetails.userName;
            var projAttached = con.projUsers.Where(p => p.user.userid == currentMemberDetails.userId);

            var catAttached = con.projCats.Where(c => c.project.ProjId == projid);

            //Query - Join Query which return a list of Rooms that to User attached to.
            var roomList = from sc in projAttached
                           join soc in catAttached
                           on sc.projid equals soc.project.ProjId
                           select new { roomName = soc.category.CatId.ToString() };

            joinChatRoom(catid.ToString());

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new ChatUserDetails { CatId = catid, userid = currentMemberDetails.userId, ConnectionId = id, UserName = currentMemberDetails.userName, projid = projid });

                // send to caller
                var messages = CurrentMessages.Where(c => c.CatId.Equals(catid)).Where(p => p.projId == projid).Select(m => new MessageDetail { UserName = m.user.UserName, Message = m.msg, Date = m.createDate.ToString() });
                Clients.Caller.onConnected(id, userName, ConnectedUsers.Where(x => x.CatId == catid), messages, projectMembersDetails);

                // send to all except caller client
                Clients.Group(catid.ToString(), id).onNewUserConnected(id, userName);
            }
        }

        /// <summary>
        /// Function which resposible to add the current connected user to a Requirment ChatRoom.
        /// </summary>
        /// <param name="roomName">The Room Name String = Requirment ID key </param>
        /// <returns>Tasks of joinChatRoom event</returns>
        private Task joinChatRoom(string roomName)
        {
            return Groups.Add(Context.ConnectionId, roomName);
        }

        /// <summary>
        /// Function which creates ChatRoom group for a Requirment.
        /// </summary>
        /// <param name="CatId">Requirment ID key </param>
        /// <param name="projId">Project ID key </param>
        /// <returns>Task of startChatRoom</returns>
        public Task startChatRoom(string catid, string projid)
        {

            return Groups.Add(Context.ConnectionId, catid);


        }

        /// <summary>
        /// Function which close a Requirment ChatRoom Group.
        /// </summary>
        /// <param name="CatId">Requirment ID key </param>
        /// <param name="projId">Project ID key</param>
        /// <returns>>Task of disconChatRoom</returns>
        public Task disconChatRoom(string catid, string projid)
        {
            return Groups.Remove(Context.ConnectionId, catid);

        }

        /// <summary>
        /// Function which send a message (string) form Client Side to the other Clients (Users) that connected to the same Requirment ChatRoom.
        /// </summary>
        /// <param name="userName">Member User Name</param>
        /// <param name="message">A texct message </param>
        public void SendMessageToAll(string userName, string message)
        {

            //Connection ID key to be uniquely recognize.
            var conId = Context.ConnectionId;
            //Contains Details about the specific User that call this function.
            var userContext = ConnectedUsers.Where(cId => cId.ConnectionId.Equals(conId)).First();


            message msg = new message();
            msg.msg = message;
            msg.createDate = DateTime.Now;
            var idd = Context.User.Identity.ToString();

            msg.userid = con.users.Find(userContext.userid).userid;
            msg.CatId = con.categories.Find(userContext.CatId).CatId;
            //var p = con.projCats.Where(c => c.catId == msg.CatId).ToList();
            msg.projId = con.projects.Find(userContext.projid).ProjId;
            con.messages.Add(msg);
            con.SaveChanges();

            // Broad cast message
            string a = userContext.CatId.ToString();
            Clients.Group(userContext.CatId.ToString()).messageReceived(userName, message, msg.createDate.ToString());
        }

        /// <summary>
        /// Override Function which responsible to deal with an event of user disconnecting the Hub.
        /// </summary>
        /// <param name="stopCalled">Bool para indicate if the disconnecting is valid or not</param>
        /// <returns>Task of OnDisconnected </returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                ChatUserDetails item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId.Equals(Context.ConnectionId));
                if (item != null)
                {
                    ConnectedUsers.Remove(item);
                    Clients.Group(item.CatId.ToString()).onUserDisconnected(item.ConnectionId, item.UserName);

                }
            }
            else
            {
                // This server hasn't heard from the client in the last ~35 seconds.
                // If SignalR is behind a load balancer with scaleout configured, 
                // the client may still be connected to another SignalR server.
            }

            return base.OnDisconnected(stopCalled);
        }


    }
}