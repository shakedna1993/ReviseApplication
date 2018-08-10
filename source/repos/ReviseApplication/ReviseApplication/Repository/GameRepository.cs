using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    /// <summary>
    /// class for getting all gamification
    /// </summary>
    public class GameRepository
    {
        /// <summary>
        /// function that gets all the gamification and puts them in a dropdown list
        /// </summary>
        /// <returns>returns select list of gamification</returns>
        public IEnumerable<SelectListItem> GetGame()
        {
            using (var context = new ReviseDBEntities())
            {
                List<SelectListItem> games = context.gamifications.AsNoTracking()
                    .OrderBy(n => n.gamId)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.gamId.ToString(),
                            Text = n.gamName
                        }).ToList();
                var GameType = new SelectListItem()
                {
                    Value = null,
                    Text = "--- select gamification ---"
                };
                games.Insert(0, GameType);
                return new SelectList(games, "Value", "Text");
            }
        }
    }
}