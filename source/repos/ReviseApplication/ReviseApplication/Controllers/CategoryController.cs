using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Models;
using ReviseApplication.Repository;

namespace ReviseApplication.Controllers
{
    /// <summary>
    /// Category contorller contains all the related pages of categories and requirements
    /// </summary>
    public class CategoryController : Controller
    {
        /// <summary>
        /// function that shows all the categories of a project
        /// calculets each category status
        /// </summary>
        /// <param name="id">the project id</param>
        /// <returns>View the categories of the project</returns>
        // GET: Category
        [HttpGet]
        public ActionResult CategoryMain(int? id)
        {
            List<user> memberslist = new List<user>();
            ReviseDBEntities con = new ReviseDBEntities();
            List<user> Allusers = new List<user>();
            List<category> catlist = con.categories.ToList();

            project proj = con.projects.Where(p => p.ProjId == id).First();
            string name = proj.ProjName.ToString();
            Session["projectName"] = name;
            Session["projectid"] = id;
            string usrid = Session["userid"].ToString();
            var prj = con.projUsers.Where(u => u.projid == id).ToList();
            var numOfMembers = con.projUsers.Where(u => u.projid == id).Count();
            
            foreach (var usr in con.users)
                Allusers.Add(usr);

            foreach (var p in prj)
                memberslist.Add(p.user);

            ViewBag.memberslist = memberslist;   //the members fo the project
            Session["MemberInProj"] = memberslist;

           foreach(var cat in catlist) //going over all the categories anf find the categories of the project
            {
                var prjcat = cat.projCats.SingleOrDefault(p => p.project.ProjId == id);
                int catid = cat.CatId;
                if (prjcat == null)
                    continue;
                    
                // the calculation of the thresholdScoreValue
                double denominator = numOfMembers * 5;
                int numerator = prjcat.score ?? 0;
                double total = Convert.ToDouble((numerator / denominator)) * 100;
                cat.projCats.SingleOrDefault(p => p.project.ProjId == id).score = Convert.ToInt32(total);
                int TotalLimit = cat.projCats.SingleOrDefault(p => p.project.ProjId == id).totalLimit ?? 0;
                int Req = 0;
                int usrCount = 0;
                int reqCount = 0;

                //the calculation of the project status
                if (con.requirements.Where(p => p.projid == prjcat.projId).SingleOrDefault(c => c.catid == catid) != null)
                     Req = con.requirements.Where(p => p.projid == prjcat.projId).SingleOrDefault(c => c.catid == catid).reqId;
                int VoteCount = con.userCatReqs.Count(r => r.reqId == Req);
                if (VoteCount < con.projUsers.Count(p => p.projid == prjcat.projId))
                    VoteCount = con.projUsers.Count(p => p.projid == prjcat.projId);
                List<userCatReq> Vote = con.userCatReqs.Where(r => r.reqId == Req).ToList();
                int? VoteSum = 0;
                foreach (var v in Vote)
                    VoteSum = VoteSum + v.rate;
                if (VoteSum != 0)
                    prjcat.status = VoteSum / VoteCount;

                if (prjcat.status == null)
                    prjcat.status = 0;

                usrCount = con.projUsers.Where(p => p.projid == prjcat.projId).Count();
                foreach(var req in con.userCatReqs.Where(r => r.reqId == Req).ToList())
                    if (req.rate != null)
                        reqCount++;

                //checks if the category is open for discussion or closed
                if (TotalLimit >= prjcat.status || usrCount > reqCount)
                    prjcat.isActive = true;

                else
                    prjcat.isActive = false;

                // if the thresholdScoreValue bigger than the thresholdScore so the requirement is aprroved
                if (total >= cat.projCats.SingleOrDefault(p => p.project.ProjId == id).score)
                    prjcat.isFinish = true;
                
                //updating the DB
                con.projCats.Find(id, catid).isActive = prjcat.isActive;
                con.projCats.Find(id, catid).isFinish = prjcat.isFinish;
                con.projCats.Find(id, catid).status = prjcat.status;
                con.SaveChanges();
            }

            var repo = new MainRepository();
            var Main = repo.CatView(Session["userid"].ToString(),id);
            return View(Main);
            //return View();
        }

        /// <summary>
        /// function that shows the create category view
        /// </summary>
        /// <param name="projid">the project id</param>
        /// <returns>create category view</returns>
        /// 
        //GET: CreateCategory 
        [HttpGet]
        public ActionResult CreateCategory(int? projid)
        {
            Session["Project"] = projid;
            return View();
        }

        /// <summary>
        /// function that shows the edit category view
        /// </summary>
        /// <param name="id">the category id </param>
        /// <param name="projid">the project id</param>
        /// <returns>edit category view</returns>
        //GET: EditCategory 
        [HttpGet]
        public ActionResult EditCategory(int? id, int projid)
        {
            var repo = new MainRepository();
            var Main = repo.CatEditView(id, projid);
            return View(Main);
        }

        /// <summary>
        /// function that shows the requirement view
        /// </summary>
        /// <returns>requirement view</returns>
        //GET: Requirement 
        [HttpGet]
        public ActionResult Requirement()
        {
            //getting the project and category id
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);

            //checks if a requirement exist
            ReviseDBEntities con = new ReviseDBEntities();
            int req = 0;
            if (con.requirements.Where(p => p.projid == projId).SingleOrDefault(c => c.catid == catId) != null)
                req = con.requirements.Where(p => p.projid == projId).SingleOrDefault(c => c.catid == catId).reqId;
            if (req == 0)
            {
                Session["ReqExisit"] = 0;
                return View();
            }
            else
            {
                Session["ReqId"] = req;
                Session["ReqExisit"] = 1;
                var repo = new MainRepository();
                var Main = repo.Requirement(req);
                return View(Main);
            }
        }

        /// <summary>
        /// function that shows the vote view
        /// </summary>
        /// <returns>vote view</returns>
        [HttpGet]
        //GET: Vote 
        public ActionResult Vote()
        {
            //gets the project, category and requirement id
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);
            int reqId = Convert.ToInt32(Session["ReqId"]);
            int IsExist = Convert.ToInt32(Session["ReqExisit"]);
            string userID = Session["userid"].ToString();

            //checks if a vote exist
            ReviseDBEntities con = new ReviseDBEntities();
            if (IsExist == 1)
            {
                var reqVote = con.userCatReqs.Find(reqId, userID);
                if (reqVote != null)
                {
                    var repo = new MainRepository();
                    var Main = repo.Vote(reqId, userID);
                    return View(Main);
                }
            }
            return View();
        }

        /// <summary>
        /// the function create requirement for a category in a project
        /// </summary>
        /// <param name="reqname"> the name of the requirement</param>
        /// <param name="reqdesc">the description of the requirement</param>
        /// <returns> redirect to the requirement view, if not everything is OK shows error</returns>
        [HttpPost]
        public ActionResult Requirement(string reqname, string reqdesc)
        {
            //gets the project and category id
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);

            //check if no requirement created
            try
            {
                if (string.IsNullOrEmpty(reqname) || string.IsNullOrEmpty(reqdesc))
                {
                    TempData["NoReq"] = "No requirement created";
                    return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
                }
            }
            catch
            {
                TempData["FailedReq"] = "Unknown error occurred!";
                return RedirectToAction("Requirement", "Category");  
            }

            //if requirement created, adding it to the DB
            if (ModelState.IsValid)
            {
                ReviseDBEntities con = new ReviseDBEntities();
                if (Convert.ToInt32(Session["ReqExisit"]) == 0)
                {
                    var req = new requirement()
                    {
                        reqName = reqname,
                        description = reqdesc,
                        projid = projId,
                        catid = catId
                    };

                    con.requirements.Add(req);
                }
                else
                {
                    int id = Convert.ToInt32(Session["ReqId"]);
                    con.requirements.SingleOrDefault(r => r.reqId == id).reqName = reqname;
                    con.requirements.SingleOrDefault(r => r.reqId == id).description = reqdesc;
                }
                
                con.SaveChanges();

                return RedirectToAction("Requirement", "Category");
            }

            return RedirectToAction("Requirement", "Category");
        }

        /// <summary>
        /// the function updates usres vote on requirement
        /// </summary>
        /// <param name="reqvote">the number the user vote</param>
        /// <returns>redirect to the same page</returns>
        [HttpPost]
        public ActionResult Vote(int reqvote)
        {
            //gets the project, category, user and requirement id
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);
            int reqId = Convert.ToInt32(Session["ReqId"]);
            int IsExist = Convert.ToInt32(Session["ReqExisit"]);
            string userID = Session["userid"].ToString();

            ReviseDBEntities con = new ReviseDBEntities();
            //checks if there is no vote
            try
            {
                if (reqvote == 0)
                {
                    TempData["EmptyVote"] = "No vote was set";
                    return RedirectToAction("Vote", "Category");
                }
            }
            catch
            {
                TempData["FailedVote"] = "Unknown error occurred!";
                return RedirectToAction("Vote", "Category");
            }

            //updaets the user vote
            if(con.userCatReqs.Find(reqId, userID) != null)
                con.userCatReqs.Find(reqId, userID).rate = reqvote;
            else
            {
                userCatReq RateReq = new userCatReq()
                {
                    usrid = userID,
                    reqId = reqId,
                    rate = reqvote
                };
                con.userCatReqs.Add(RateReq);
            }


            con.SaveChanges();

            return RedirectToAction("Vote", "Category");
        }

        /// <summary>
        /// function that updates changes of a category
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="catname">categry name</param>
        /// <param name="totalLimit">the limit of the category</param>
        /// <returns>if everything is OK redirect to category main, if not redirect to category edit with error</returns>
        [HttpPost]
        public ActionResult EditCategory(int ? id, string catname, int ? totalLimit)
        {
            //checks if there are empty fields
            try
            {
                if (string.IsNullOrEmpty(catname) || totalLimit == null)
                {
                    TempData["EmptyFields"] = "One or more fields are empty";
                    return RedirectToAction("EditCategory", "Category");
                }
            }
            catch
            {
                TempData["FailedVote"] = "Unknown error occurred!";
                return RedirectToAction("EditCategory", "Category");
            }

            //gets category information from the DB
            ReviseDBEntities con = new ReviseDBEntities();
            var CatName = con.categories.Find(id).CatName;
            int projid = Convert.ToInt32(Session["projectid"]);
            var CatLimit = con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid).totalLimit ?? 0;

            // checks if there were no changes, checks if the category already exist
            try
            {
                if (CatName == catname && CatLimit == totalLimit)
                {
                    TempData["NoChanges"] = "No changes made";
                    return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
                }
                if(CatName != catname && con.projCats.Where(p => p.projId == projid).Any(c => c.category.CatName == catname))
                {
                    TempData["CatExist"] = "Category with this name already exist";
                    return RedirectToAction("EditCategory", "Category", new { id = id, projid = projid });
                }
                   
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("EditCategory", "Category", new { id = id, projid = projid });
            }

            //save changes to the DB
            if (ModelState.IsValid)
            {
                con.categories.Find(id).CatName = catname;
                con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid).totalLimit = totalLimit;
                con.SaveChanges();
                return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
            }
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }

        /// <summary>
        /// function create new category and link it to the specific project
        /// </summary>
        /// <param name="catname"> category name provided</param>
        /// <param name="totalLimit">category limit provided</param>
        /// <returns>if created successfully, redirect to category main, if not, redirect to create category again</returns>
        [HttpPost]
        public ActionResult CreateCategory(string catname, int? totalLimit)
        {
            int projid = Convert.ToInt32(Session["projectid"]);    //gets the project id
            //checks if one of the fields are empty
            try
            {
                if (string.IsNullOrEmpty(catname) || totalLimit == null)
                {
                    TempData["EmptyFields"] = "One or more field is empty";
                    return RedirectToAction("CreateCategory", "Category");
                }
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("CreateCategory", "Category");
            }

            //update the DB, creating new category
            if (ModelState.IsValid)
            {
                int total = 0;
                ReviseDBEntities con = new ReviseDBEntities();
                var catTemp = con.categories.SingleOrDefault(c => c.CatName == catname);
                int catid = 0;
                if (catTemp != null)
                {
                    catid = catTemp.CatId;
                    var prjcat = con.projCats.Where(c => c.catId == catid).SingleOrDefault(p => p.projId == projid);
                    if(prjcat != null)
                    {
                        TempData["FailedCat"] = "Category with this name already exist";
                        return RedirectToAction("CreateCategory", "Category", new { id = projid });
                    }
                    else
                    {
                        total = totalLimit ?? 0;
                        var projcat = new projCat  //link the category to the specific project
                        {
                            catId = catid,
                            projId = projid,
                            totalLimit = total
                        };
                        con.projCats.Add(projcat);
                    }
                }
                else
                {
                    var cat = new category
                    {
                        CatName = catname
                    };

                    foreach (var prj in con.projects)
                    {
                        if (prj.ProjId != projid)
                            total = 0;
                        else
                            total = totalLimit ?? 0;

                        var prjcat = new projCat
                        {
                            catId = cat.CatId,
                            projId = prj.ProjId,
                            totalLimit = total
                        };
                        con.projCats.Add(prjcat);
                    }

                    con.categories.Add(cat);
                }
                try
                {
                    
                    con.SaveChanges();
                }
                catch (DbUpdateException)   //if the category already exist
                {
                    TempData["FailedCat"] = "Category with this name already exist";
                    return RedirectToAction("CreateCategory", "Category", new { id = projid });
                }

                return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
            }
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }


        /// <summary>
        /// function that delete specific category
        /// </summary>
        /// <param name="id">the category id for delete</param>
        /// <returns>returns to the category main</returns>
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["emptyIDcat"] = "An error occurred while trying to delete";
                return RedirectToAction("CategoryMain", "Category", new { id = Convert.ToInt32(Session["projectid"]), name = Session["projectName"].ToString() });
            }
            var repo = new MainRepository();
            var Main = repo.CatEditView(id, Convert.ToInt32(Session["projectid"]));
            return View(Main);
        }

        /// <summary>
        /// function that delete specific category
        /// </summary>
        /// <param name="id">the category id for delete</param>
        /// <returns>pop up message to verify delete</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            int projid = Convert.ToInt32(Session["projectid"]);
            ReviseDBEntities con = new ReviseDBEntities();

            //finds the category in DB, delete it from the specific project
            category cat = con.categories.Find(id);
            var catprj = con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid);
            con.projCats.Remove(catprj);

            var catreq = con.requirements.Where(c => c.catid == id).SingleOrDefault(p => p.projid == projid);
            if(catreq !=null)
                con.requirements.Remove(catreq);

            var message = con.messages.Where(c => c.CatId == id).Where(p => p.projId == projid).ToList();
            if (message != null)
            {
                foreach (var msg in message)
                    con.messages.Remove(msg);
            }

            con.SaveChanges();
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }
    }
}