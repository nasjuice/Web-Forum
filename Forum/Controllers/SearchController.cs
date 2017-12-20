using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Forum.Models;

namespace Forum.Controllers
{
    public class SearchController : Controller
    {
        private ForumDb db = new ForumDb();

        private String input = "";

        // GET: Search
        public ActionResult Index(String keyword)
        {
            if (keyword == null || keyword == "")
                return RedirectToAction("Index", "Home");

            input = keyword;

            List<Thread> threads = getMatchingThread();

            if (threads.Count == 0)
                return RedirectToAction("Index", "Home");
            else
                return View(threads);
        }


        private List<Thread> getMatchingThread()
        {
            List<Thread> titles = getMatchingTitle();
            List<Thread> content = getMatchingContent();
            List<Thread> tags = getMatchingTags();

            List<Thread> threads = titles.Concat(content).Concat(tags).Distinct().Take(20).ToList<Thread>();

            return threads;
        }

        private List<Thread> getMatchingTitle()
        {
            List<Thread> matchedThread = (from t in db.Threads
                                          where t.Title.IndexOf(input) != -1
                                          orderby t.Views descending
                                          select t).Take(20).ToList<Thread>();

            return matchedThread;
        }

        private List<Thread> getMatchingTags()
        {
            List<Thread> matchedThread = (from t in db.TagThreads
                                          where t.Tag.TagText.IndexOf(input) != -1
                                          orderby t.Thread.Views
                                          select t.Thread).Take(20).ToList<Thread>();

            return matchedThread;
        }

        private List<Thread> getMatchingContent()
        {
            List<Thread> matchedThread = (from t in db.Threads
                                          where t.Content.IndexOf(input) != -1
                                          orderby t.Views descending
                                          select t).Take(20).ToList<Thread>();

            return matchedThread;
        }

      

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
