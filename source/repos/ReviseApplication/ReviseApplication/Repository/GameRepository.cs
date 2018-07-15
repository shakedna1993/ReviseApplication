using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    public class GameRepository
    {
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