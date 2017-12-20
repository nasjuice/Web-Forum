using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Forum.Models;
using Forum.Validators;

namespace Forum.Controllers
{
    public class HomeController : Controller
    {
        private ForumDb db = new ForumDb();

        // GET: Home
        public ActionResult Index()
        {
            var threads = db.Threads.Include(t => t.User);
            return View(threads.ToList());
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }

            List<Comment> comments = (from c in db.Comments
                                      where c.ThreadId == id && c.ParentCommentId == null
                                      select c).ToList<Comment>();
            ViewBag.Comment = comments;

            thread.Views++;
            db.SaveChanges();
            return View(thread);
        }


        [Authorize]
        public ActionResult Upvote(int id)
        {
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }

            thread.Upvotes++;

            db.Entry(thread).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        // GET: Home/Create
        [Authorize]
        public ActionResult Create()
        {

            Thread thread = new Thread();

            List<Tag> tags = (from t in db.Tags
                              select t).ToList<Tag>();
            ViewBag.TagsInThread = tags;


            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password");
            return View(thread);
        }

        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ThreadId,Title,Content,UserName,Date,Views,Upvotes")] Thread thread, IEnumerable<int> tagsId)
        {
            if (ModelState.IsValid)
            {
                DateTime localDate = DateTime.Now;

                thread.UserName = System.Web.HttpContext.Current.User.Identity.Name;
                thread.Date = localDate;
                db.Threads.Add(thread);

                TagThread tagthread;
                if (tagsId != null)
                {
                    foreach (var t in tagsId)
                    {
                        tagthread = new TagThread();
                        tagthread.TagId = t;
                        tagthread.ThreadId = thread.ThreadId;

                        db.TagThreads.Add(tagthread);
                    }
                }


                db.SaveChanges();

                return RedirectToAction("Details", "Home", new { id = thread.ThreadId });
            }

            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", thread.UserName);
            return View(thread);
        }

        // GET: Home/Edit/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", thread.UserName);
            return View(thread);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ThreadId,Title,Content,UserName,Date,Views,Upvotes")] Thread thread)
        {
            if (ModelState.IsValid)
            {
                DateTime localDate = DateTime.Now;

                thread.UserName = System.Web.HttpContext.Current.User.Identity.Name;
                thread.Date = localDate;
                db.Threads.Add(thread);


                db.Entry(thread).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", thread.UserName);
            return View(thread);
        }

        // GET: Home/Delete/5
        [AdminAuthorize(Users = "admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Thread thread = db.Threads.Find(id);
            if (thread == null)
            {
                return HttpNotFound();
            }
            return View(thread);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Thread thread = db.Threads.Find(id);
            List<TagThread> list = (from t in db.TagThreads
                                    where t.ThreadId == id
                                    select t).ToList<TagThread>();

            foreach (var tagthread in list)
            {
                db.TagThreads.Remove(tagthread);
            }

            List<Comment> comment = (from c in db.Comments
                                     where c.ThreadId == id
                                     select c).ToList<Comment>();

            foreach (var c in comment)
            {
                db.Comments.Remove(c);
            }

            db.Threads.Remove(thread);
            db.SaveChanges();
            return RedirectToAction("Index","Home");
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
