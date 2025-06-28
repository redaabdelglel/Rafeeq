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

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<ChatConversation> ChatConversations { get; set; }
    public virtual DbSet<MessageReadStatus> MessageReadStatuses { get; set; }
    public virtual DbSet<MessageReaction> MessageReactions { get; set; }
    public virtual DbSet<Article> Articles { get; set; }
    public virtual DbSet<FAQ> FAQs { get; set; }

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
            entity.HasKey(e => e.AvailabilityId).HasName("PK_Availabi_DA3979B1C654E814");

            entity.HasOne(d => d.User).WithMany(p => p.Availabilities).HasConstraintName("FK_AvailabilUserI_4CA06362");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK_Bookings_73951AED008D420C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PaymentStatus).HasDefaultValue("Unpaid");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Mentee).WithMany(p => p.BookingMentees).HasConstraintName("FK_BookingsMentee_5070F446");

            entity.HasOne(d => d.Mentor).WithMany(p => p.BookingMentors).HasConstraintName("FK_BookingsMentor_4F7CD00D");
        });

        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK_ChatAtta_442C64BE4F573056");

            entity.HasOne(d => d.Message).WithMany(p => p.ChatAttachments).HasConstraintName("FK_ChatAttacMessa_70DDC3D8");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK_ChatMess_C87C0C9CDDF239B2");

            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.ChatMessages).HasConstraintName("FK_ChatMessaBooki_5DCAEF64");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessages).HasConstraintName("FK_ChatMessaSende_5EBF139D");
            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
               .HasConstraintName("FK_ChatMessaConve_XXXXX");
        });

        modelBuilder.Entity<MenteeSkill>(entity =>
        {
            entity.HasKey(e => e.MenteeSkillId).HasName("PK_MenteeSk_8AC70FF0D5795623");

            entity.HasOne(d => d.Skill).WithMany(p => p.MenteeSkills).HasConstraintName("FK_MenteeSkiSkill_49C3F6B7");

            entity.HasOne(d => d.User).WithMany(p => p.MenteeSkills).HasConstraintName("FK_MenteeSkiUserI_48CFD27E");
        });

        modelBuilder.Entity<MentorSkill>(entity =>
        {
            entity.HasKey(e => e.MentorSkillId).HasName("PK_MentorSk_814986AD639F79B4");

            entity.HasOne(d => d.Skill).WithMany(p => p.MentorSkills).HasConstraintName("FK_MentorSkiSkill_45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.MentorSkills).HasConstraintName("FK_MentorSkiUserI_44FF419A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK_Notifica_20CF2E12817FB374");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("FK_NotificatUserI_6754599E");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK_Payments_9B556A383E8C0C2F");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments).HasConstraintName("FK_PaymentsBookin_6383C8BA");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK_Reviews_74BC79CEE8C74737");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews).HasConstraintName("FK_ReviewsBooking_59063A47");

            entity.HasOne(d => d.ReviewedUser).WithMany(p => p.ReviewReviewedUsers).HasConstraintName("FK_ReviewsReviewe_5812160E");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers).HasConstraintName("FK_ReviewsReviewe_571DF1D5");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_Roles_8AFACE1A4662431D");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK_Skills_DFA0918765B9AA60");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Users_1788CC4C54AD77F5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsInterviewer).HasDefaultValue(false);
            entity.Property(e => e.IsMentor).HasDefaultValue(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK_UsersRoleId_3B75D760");
        });
        modelBuilder.Entity<MenteeCV>(entity =>
        {
            entity.HasKey(e => e.CVId);
            entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(d => d.User).WithMany(p => p.CVs).HasConstraintName("FK_MenteeCVsUserI_XXXX");
        });

        modelBuilder.Entity<CVComment>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.CV).WithMany(p => p.Comments).HasConstraintName("FK_CVCommentCVId_XXXX");
            entity.HasOne(d => d.Mentor).WithMany(p => p.CVComments).HasConstraintName("FK_CVCommentMentor_XXXX");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK_UserToke_658FEEEAA975E301");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens).HasConstraintName("FK_UserTokenUserI_6C190EBB");
        });


        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("New");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Responder)
                .WithMany()
                .HasForeignKey(d => d.RespondedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
        // Configure ChatConversations entity
        modelBuilder.Entity<ChatConversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK_ChatConv_C95C11C4E86EA4A7");

            entity.Property(e => e.LastMessageAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany()
                .HasConstraintName("FK_ChatConvBooki_XXXXX");

            entity.HasOne(d => d.Mentor).WithMany()
                .HasConstraintName("FK_ChatConvMento_XXXXX");

            entity.HasOne(d => d.Mentee).WithMany()
                .HasConstraintName("FK_ChatConvMente_XXXXX");
        });
        // Configure MessageReadStatus entity
        modelBuilder.Entity<MessageReadStatus>(entity =>
        {
            entity.HasKey(e => e.ReadStatusId).HasName("PK_MessageR_B19EAD9154C14476");
            entity.ToTable("MessageReadStatus");

            entity.Property(e => e.ReadAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Message).WithMany(p => p.ReadStatuses)
                .HasConstraintName("FK_MessageReMessa_XXXXX");

            entity.HasOne(d => d.User).WithMany()
                .HasConstraintName("FK_MessageReUserI_XXXXX");
        });
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleId);
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Author)
                  .WithMany(p => p.Articles)
                  .HasForeignKey(d => d.AuthorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FAQ>(entity => 
        {
            entity.ToTable("FAQ"); 
            entity.HasKey(e => e.FAQId);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}