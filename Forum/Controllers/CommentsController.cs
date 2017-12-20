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
    public class CommentsController : Controller
    {
        private ForumDb db = new ForumDb();

        // GET: Comments
        public ActionResult Index()
        {
            var comments = db.Comments.Include(c => c.Comment2).Include(c => c.Thread).Include(c => c.User);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        [Authorize]
        public ActionResult Create(int? id)
        {
            Comment comment = new Comment();
            comment.ThreadId = (int)id;
            DateTime localDate = DateTime.Now;
            comment.Date = localDate;
            comment.UserName = System.Web.HttpContext.Current.User.Identity.Name;

            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName");
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title");
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password");
            return View(comment);
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CommentId,ThreadId,ParentCommentId,UserName,Date,Content")] Comment comment)
        {


            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = comment.ThreadId });
            }

            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName", comment.ParentCommentId);
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title", comment.ThreadId);
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", comment.UserName);
            return View(comment);
        }

        // GET: Comments/Create
        [Authorize]
        public ActionResult ReplyToComment(int? id)
        {
            Comment parent = (from c in db.Comments
                              where c.CommentId == id
                              select c).FirstOrDefault<Comment>();
            Comment comment = new Comment();
            comment.ParentCommentId = parent.CommentId;
            comment.ThreadId = parent.ThreadId;
            DateTime localDate = DateTime.Now;
            comment.Date = localDate;
            comment.UserName = System.Web.HttpContext.Current.User.Identity.Name;

            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName");
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title");
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password");
            return View(comment);
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReplyToComment([Bind(Include = "CommentId,ThreadId,ParentCommentId,UserName,Date,Content")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = comment.ThreadId });
            }

            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName", comment.ParentCommentId);
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title", comment.ThreadId);
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", comment.UserName);
            return View(comment);
        }


        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            DateTime localDate = DateTime.Now;
            comment.Date = localDate;

            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName", comment.ParentCommentId);
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title", comment.ThreadId);
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", comment.UserName);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CommentId,ThreadId,ParentCommentId,UserName,Date,Content")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentCommentId = new SelectList(db.Comments, "CommentId", "UserName", comment.ParentCommentId);
            ViewBag.ThreadId = new SelectList(db.Threads, "ThreadId", "Title", comment.ThreadId);
            ViewBag.UserName = new SelectList(db.Users, "UserName", "Password", comment.UserName);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);

            deleteSubComment(comment);

            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = comment.ThreadId });

        }

        public void deleteSubComment(Comment subComment)
        {
            if (subComment != null)
            {
                var comment = (from c in db.Comments
                               where c.ParentCommentId == subComment.CommentId
                               select c).ToList<Comment>();

                if (comment.Count > 0)
                {
                    foreach (var c in comment)
                    {
                        deleteSubComment(c);
                    }
                }
                db.Comments.Remove(subComment);
            }

            return;

          
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
