
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Models;

public partial class RafeeqContext : DbContext
{
    public RafeeqContext()
    {
    }

    public RafeeqContext(DbContextOptions<RafeeqContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<ChatAttachment> ChatAttachments { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<MenteeSkill> MenteeSkills { get; set; }

    public virtual DbSet<MentorSkill> MentorSkills { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }
    public virtual DbSet<MenteeCV> MenteeCVs { get; set; }
    public virtual DbSet<CVComment> CVComments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=db20643.public.databaseasp.net; Database=db20643; User Id=db20643; Password=tD@2-b4KxQ?3; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");
        }
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("PK__Availabi__DA3979B1C654E814");

            entity.HasOne(d => d.User).WithMany(p => p.Availabilities).HasConstraintName("FK__Availabil__UserI__4CA06362");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AED008D420C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PaymentStatus).HasDefaultValue("Unpaid");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Mentee).WithMany(p => p.BookingMentees).HasConstraintName("FK__Bookings__Mentee__5070F446");

            entity.HasOne(d => d.Mentor).WithMany(p => p.BookingMentors).HasConstraintName("FK__Bookings__Mentor__4F7CD00D");
        });

        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__ChatAtta__442C64BE4F573056");

            entity.HasOne(d => d.Message).WithMany(p => p.ChatAttachments).HasConstraintName("FK__ChatAttac__Messa__70DDC3D8");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ChatMess__C87C0C9CDDF239B2");

            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.ChatMessages).HasConstraintName("FK__ChatMessa__Booki__5DCAEF64");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessages).HasConstraintName("FK__ChatMessa__Sende__5EBF139D");
        });

        modelBuilder.Entity<MenteeSkill>(entity =>
        {
            entity.HasKey(e => e.MenteeSkillId).HasName("PK__MenteeSk__8AC70FF0D5795623");

            entity.HasOne(d => d.Skill).WithMany(p => p.MenteeSkills).HasConstraintName("FK__MenteeSki__Skill__49C3F6B7");

            entity.HasOne(d => d.User).WithMany(p => p.MenteeSkills).HasConstraintName("FK__MenteeSki__UserI__48CFD27E");
        });

        modelBuilder.Entity<MentorSkill>(entity =>
        {
            entity.HasKey(e => e.MentorSkillId).HasName("PK__MentorSk__814986AD639F79B4");

            entity.HasOne(d => d.Skill).WithMany(p => p.MentorSkills).HasConstraintName("FK__MentorSki__Skill__45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.MentorSkills).HasConstraintName("FK__MentorSki__UserI__44FF419A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12817FB374");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__UserI__6754599E");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A383E8C0C2F");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments).HasConstraintName("FK__Payments__Bookin__6383C8BA");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CEE8C74737");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews).HasConstraintName("FK__Reviews__Booking__59063A47");

            entity.HasOne(d => d.ReviewedUser).WithMany(p => p.ReviewReviewedUsers).HasConstraintName("FK__Reviews__Reviewe__5812160E");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers).HasConstraintName("FK__Reviews__Reviewe__571DF1D5");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A4662431D");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skills__DFA0918765B9AA60");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C54AD77F5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsInterviewer).HasDefaultValue(false);
            entity.Property(e => e.IsMentor).HasDefaultValue(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK__Users__RoleId__3B75D760");
        });
        modelBuilder.Entity<MenteeCV>(entity =>
        {
            entity.HasKey(e => e.CVId);
            entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(d => d.User).WithMany(p => p.CVs).HasConstraintName("FK__MenteeCVs__UserI__XXXX");
        });

        modelBuilder.Entity<CVComment>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.CV).WithMany(p => p.Comments).HasConstraintName("FK__CVComment__CVId__XXXX");
            entity.HasOne(d => d.Mentor).WithMany(p => p.CVComments).HasConstraintName("FK__CVComment__Mentor__XXXX");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__UserToke__658FEEEAA975E301");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens).HasConstraintName("FK__UserToken__UserI__6C190EBB");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}