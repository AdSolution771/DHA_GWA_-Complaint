using Microsoft.EntityFrameworkCore;
using System;

namespace WebUI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }
        public DbSet<ComplaintCategory> ComplaintCategories { get; set; }
        public DbSet<ComplaintFeedback> ComplaintFeedbacks { get; set; }
        public DbSet<ComplaintHistory> ComplaintHistories { get; set; }
        public DbSet<ComplaintIndicator> ComplaintIndicators { get; set; }
        public DbSet<ComplaintStatus> ComplaintStatuses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// Prevent multiple cascade paths for ComplaintStatus → Category
            //modelBuilder.Entity<ComplaintStatus>()
            //    .HasOne(s => s.Category)
            //    .WithMany()
            //    .HasForeignKey(s => s.CategoryId)
            //    .OnDelete(DeleteBehavior.Restrict); // Important!

            // ComplaintHistory -> Complaint (disable cascade to avoid multiple paths)
            modelBuilder.Entity<ComplaintHistory>()
                .HasOne(ch => ch.Complaint)
                .WithMany(c => c.Histories)
                .HasForeignKey(ch => ch.ComplaintId)
                .OnDelete(DeleteBehavior.Restrict);

            // ComplaintHistory -> ComplaintStatus (OldStatus)
            modelBuilder.Entity<ComplaintHistory>()
                .HasOne(ch => ch.OldStatus)
                .WithMany()
                .HasForeignKey(ch => ch.OldStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            // ComplaintHistory -> ComplaintStatus (NewStatus)
            modelBuilder.Entity<ComplaintHistory>()
                .HasOne(ch => ch.NewStatus)
                .WithMany()
                .HasForeignKey(ch => ch.NewStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            // ComplaintHistory -> Users (AssignedBy)
            modelBuilder.Entity<ComplaintHistory>()
                .HasOne(ch => ch.AssignedBy)
                .WithMany()
                .HasForeignKey(ch => ch.AssignedById)
                .OnDelete(DeleteBehavior.NoAction);

            // ComplaintHistory -> Users (AssignedTo)
            modelBuilder.Entity<ComplaintHistory>()
                .HasOne(ch => ch.AssignedTo)
                .WithMany()
                .HasForeignKey(ch => ch.AssignedToId)
                .OnDelete(DeleteBehavior.NoAction);

            // Keep cascade only where safe (e.g. Complaint -> Attachments)
            modelBuilder.Entity<ComplaintAttachment>()
                .HasOne(a => a.Complaint)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);


            // ComplaintFeedback -> Complaint
            modelBuilder.Entity<ComplaintFeedback>()
                .HasOne(f => f.Complaint)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade); // safe, when complaint deleted, feedbacks go too

            // ComplaintFeedback -> User (FeedbackBy)
            modelBuilder.Entity<ComplaintFeedback>()
                .HasOne(f => f.FeedbackBy)
                .WithMany()
                .HasForeignKey(f => f.FeedbackById)
                .OnDelete(DeleteBehavior.Restrict); // 🚨 change from Cascade to Restrict


            // ComplaintAttachment -> Complaint
            modelBuilder.Entity<ComplaintAttachment>()
                .HasOne(a => a.Complaint)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade); // delete attachments when complaint deleted

            // ComplaintAttachment -> User (UploadedBy)
            modelBuilder.Entity<ComplaintAttachment>()
                .HasOne(a => a.UploadedBy)
                .WithMany()
                .HasForeignKey(a => a.UploadedById)
                .OnDelete(DeleteBehavior.Restrict); // 🚨 change Cascade → Restrict

            // ComplaintAttachment -> ComplaintHistory (optional, HistoryId can be null)
            modelBuilder.Entity<ComplaintAttachment>()
                .HasOne(a => a.History)
                .WithMany(h => h.Attachments)
                .HasForeignKey(a => a.HistoryId)
                .OnDelete(DeleteBehavior.Restrict); // safer than cascade

            //UserPlayerId configuration
           

        }

    }
}
