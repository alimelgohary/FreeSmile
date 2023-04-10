using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using FreeSmile.Models;
using FreeSmile.Services;

namespace FreeSmile
{
    public partial class FreeSmileContext : DbContext
    {
        public FreeSmileContext()
        {
        }

        public FreeSmileContext(DbContextOptions<FreeSmileContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AcademicDegree> AcademicDegrees { get; set; } = null!;
        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Article> Articles { get; set; } = null!;
        public virtual DbSet<ArticleCat> ArticleCats { get; set; } = null!;
        public virtual DbSet<Case> Cases { get; set; } = null!;
        public virtual DbSet<CaseType> CaseTypes { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<CommentReport> CommentReports { get; set; } = null!;
        public virtual DbSet<Dentist> Dentists { get; set; } = null!;
        public virtual DbSet<Governate> Governates { get; set; } = null!;
        public virtual DbSet<Listing> Listings { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
        public virtual DbSet<Patient> Patients { get; set; } = null!;
        public virtual DbSet<Portfolio> Portfolios { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<PostImage> PostImages { get; set; } = null!;
        public virtual DbSet<PostReport> PostReports { get; set; } = null!;
        public virtual DbSet<ProductCat> ProductCats { get; set; } = null!;
        public virtual DbSet<Review> Reviews { get; set; } = null!;
        public virtual DbSet<SharingPatient> SharingPatients { get; set; } = null!;
        public virtual DbSet<SuperAdmin> SuperAdmins { get; set; } = null!;
        public virtual DbSet<University> Universities { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<VerificationRequest> VerificationRequests { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(MyConstants.FREESMILE_CONNECTION);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcademicDegree>(entity =>
            {
                entity.HasKey(e => e.DegId)
                    .HasName("PK__academic__2EB5580C6E9447BA");

                entity.ToTable("academicDegrees");

                entity.HasIndex(e => e.NameEn, "UQ__academic__F48030FABFB47E36")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__academic__F480526D26D5A3D0")
                    .IsUnique();

                entity.Property(e => e.DegId).HasColumnName("deg_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(20)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.Property(e => e.AdminId)
                    .ValueGeneratedNever()
                    .HasColumnName("admin_id");

                entity.HasOne(d => d.AdminNavigation)
                    .WithOne(p => p.Admin)
                    .HasForeignKey<Admin>(d => d.AdminId)
                    .HasConstraintName("FK__admins__admin_id__10AB74EC");
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("articles");

                entity.Property(e => e.ArticleId)
                    .ValueGeneratedNever()
                    .HasColumnName("article_id");

                entity.Property(e => e.CatId)
                    .HasColumnName("cat_id")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.ArticleNavigation)
                    .WithOne(p => p.Article)
                    .HasForeignKey<Article>(d => d.ArticleId)
                    .HasConstraintName("FK__articles__articl__351DDF8C");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.CatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__articles__cat_id__361203C5");
            });

            modelBuilder.Entity<ArticleCat>(entity =>
            {
                entity.ToTable("articleCats");

                entity.HasIndex(e => e.NameEn, "UQ__articleC__F48030FA1336BDEA")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__articleC__F480526D9A4B5B7C")
                    .IsUnique();

                entity.Property(e => e.ArticleCatId).HasColumnName("article_cat_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(50)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");
            });

            modelBuilder.Entity<Case>(entity =>
            {
                entity.ToTable("cases");

                entity.Property(e => e.CaseId)
                    .ValueGeneratedNever()
                    .HasColumnName("case_id");

                entity.Property(e => e.CaseTypeId)
                    .HasColumnName("case_type_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.GovernateId).HasColumnName("governate_id");

                entity.HasOne(d => d.CaseNavigation)
                    .WithOne(p => p.Case)
                    .HasForeignKey<Case>(d => d.CaseId)
                    .HasConstraintName("FK__cases__case_id__51EF2864");

                entity.HasOne(d => d.CaseType)
                    .WithMany(p => p.Cases)
                    .HasForeignKey(d => d.CaseTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__cases__case_type__54CB950F");

                entity.HasOne(d => d.Governate)
                    .WithMany(p => p.Cases)
                    .HasForeignKey(d => d.GovernateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__cases__governate__55BFB948");
            });

            modelBuilder.Entity<CaseType>(entity =>
            {
                entity.ToTable("caseTypes");

                entity.HasIndex(e => e.NameEn, "UQ__caseType__F48030FADA92B16C")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__caseType__F480526DD625D2F6")
                    .IsUnique();

                entity.Property(e => e.CaseTypeId).HasColumnName("case_type_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(100)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments");

                entity.Property(e => e.CommentId).HasColumnName("comment_id");

                entity.Property(e => e.ArticleId).HasColumnName("article_id");

                entity.Property(e => e.Body)
                    .HasMaxLength(500)
                    .HasColumnName("body");

                entity.Property(e => e.TimeWritten)
                    .HasColumnType("datetime")
                    .HasColumnName("timeWritten")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__comments__articl__40E497F3");

                entity.HasOne(d => d.Writer)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.WriterId)
                    .HasConstraintName("FK__comments__writer__3FF073BA");
            });

            modelBuilder.Entity<CommentReport>(entity =>
            {
                entity.HasKey(e => new { e.ReporterId, e.CommentId })
                    .HasName("PK_comments_reports");

                entity.ToTable("commentReports");

                entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

                entity.Property(e => e.CommentId).HasColumnName("comment_id");

                entity.Property(e => e.Reason)
                    .HasMaxLength(100)
                    .HasColumnName("reason");

                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.CommentReports)
                    .HasForeignKey(d => d.CommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__commentRe__comme__44B528D7");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.CommentReports)
                    .HasForeignKey(d => d.ReporterId)
                    .HasConstraintName("FK__commentRe__repor__43C1049E");
            });

            modelBuilder.Entity<Dentist>(entity =>
            {
                entity.ToTable("dentists");

                entity.Property(e => e.DentistId)
                    .ValueGeneratedNever()
                    .HasColumnName("dentist_id");

                entity.Property(e => e.Bio)
                    .HasMaxLength(100)
                    .HasColumnName("bio");

                entity.Property(e => e.CurrentDegree)
                    .HasColumnName("currentDegree")
                    .HasDefaultValueSql("((2))");

                entity.Property(e => e.CurrentUniversity)
                    .HasColumnName("current_university")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.FbUsername)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fbUsername");

                entity.Property(e => e.GScholarUsername)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("gScholarUsername");

                entity.Property(e => e.IsVerifiedDentist).HasColumnName("isVerifiedDentist");

                entity.Property(e => e.LinkedUsername)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("linkedUsername");

                entity.HasOne(d => d.CurrentDegreeNavigation)
                    .WithMany(p => p.Dentists)
                    .HasForeignKey(d => d.CurrentDegree)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__dentists__curren__035179CE");

                entity.HasOne(d => d.CurrentUniversityNavigation)
                    .WithMany(p => p.Dentists)
                    .HasForeignKey(d => d.CurrentUniversity)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__dentists__curren__04459E07");

                entity.HasOne(d => d.DentistNavigation)
                    .WithOne(p => p.Dentist)
                    .HasForeignKey<Dentist>(d => d.DentistId)
                    .HasConstraintName("FK__dentists__dentis__025D5595");
            });

            modelBuilder.Entity<Governate>(entity =>
            {
                entity.HasKey(e => e.GovId)
                    .HasName("PK__governat__A7D8D2EBD36EF5F2");

                entity.ToTable("governates");

                entity.HasIndex(e => e.NameEn, "UQ__governat__F48030FAC9756789")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__governat__F480526D89BA78F1")
                    .IsUnique();

                entity.Property(e => e.GovId).HasColumnName("gov_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(50)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");
            });

            modelBuilder.Entity<Listing>(entity =>
            {
                entity.ToTable("listings");

                entity.Property(e => e.ListingId)
                    .ValueGeneratedNever()
                    .HasColumnName("listing_id");

                entity.Property(e => e.CatId)
                    .HasColumnName("cat_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.GovernateId).HasColumnName("governate_id");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Listings)
                    .HasForeignKey(d => d.CatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__listings__cat_id__3CBF0154");

                entity.HasOne(d => d.Governate)
                    .WithMany(p => p.Listings)
                    .HasForeignKey(d => d.GovernateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__listings__govern__3BCADD1B");

                entity.HasOne(d => d.ListingNavigation)
                    .WithOne(p => p.Listing)
                    .HasForeignKey<Listing>(d => d.ListingId)
                    .HasConstraintName("FK__listings__listin__39E294A9");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");

                entity.Property(e => e.MessageId).HasColumnName("message_id");

                entity.Property(e => e.Body)
                    .HasMaxLength(300)
                    .HasColumnName("body");

                entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");

                entity.Property(e => e.Seen).HasColumnName("seen");

                entity.Property(e => e.SenderId).HasColumnName("sender_id");

                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime")
                    .HasColumnName("sentAt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.MessageReceivers)
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__messages__receiv__208CD6FA");

                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.MessageSenders)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__messages__sender__1F98B2C1");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.Property(e => e.NotificationId).HasColumnName("notification_id");

                entity.Property(e => e.ActorUsername)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("actor_username");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.Property(e => e.PostTitle)
                    .HasMaxLength(20)
                    .HasColumnName("post_title");

                entity.Property(e => e.Seen)
                    .HasColumnName("seen")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime")
                    .HasColumnName("sentAt")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.TempId).HasColumnName("temp_id");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("FK__notificat__owner__0C70CFB4");

                entity.HasOne(d => d.Temp)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.TempId)
                    .HasConstraintName("FK__notificat__temp___0F4D3C5F");
            });

            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.TempId)
                    .HasName("PK__notifica__FEEC6BDB93C9075C");

                entity.ToTable("notificationTemplates");

                entity.HasIndex(e => e.TempName, "UQ__notifica__C40A32D12594CD76")
                    .IsUnique();

                entity.Property(e => e.TempId).HasColumnName("temp_id");

                entity.Property(e => e.BodyAr)
                    .HasMaxLength(200)
                    .HasColumnName("bodyAr");

                entity.Property(e => e.BodyEn)
                    .HasMaxLength(200)
                    .HasColumnName("bodyEn");

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("icon");

                entity.Property(e => e.NextPage)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("nextPage")
                    .HasDefaultValueSql("('same')");

                entity.Property(e => e.TempName)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("temp_name");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");

                entity.Property(e => e.PatientId)
                    .ValueGeneratedNever()
                    .HasColumnName("patient_id");

                entity.HasOne(d => d.PatientNavigation)
                    .WithOne(p => p.Patient)
                    .HasForeignKey<Patient>(d => d.PatientId)
                    .HasConstraintName("FK__patients__patien__147C05D0");
            });

            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.ToTable("portfolios");

                entity.Property(e => e.PortfolioId).HasColumnName("portfolio_id");

                entity.Property(e => e.AfterImage)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("afterImage");

                entity.Property(e => e.BeforeImage)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("beforeImage");

                entity.Property(e => e.CaseDescription)
                    .HasMaxLength(100)
                    .HasColumnName("caseDescription");

                entity.Property(e => e.DentistId).HasColumnName("dentist_id");

                entity.HasOne(d => d.Dentist)
                    .WithMany(p => p.Portfolios)
                    .HasForeignKey(d => d.DentistId)
                    .HasConstraintName("FK__portfolio__denti__0AF29B96");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.Property(e => e.Body)
                    .HasMaxLength(500)
                    .HasColumnName("body");

                entity.Property(e => e.TimeUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("timeUpdated");

                entity.Property(e => e.TimeWritten)
                    .HasColumnType("datetime")
                    .HasColumnName("timeWritten")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Title)
                    .HasMaxLength(20)
                    .HasColumnName("title");

                entity.Property(e => e.WriterId).HasColumnName("writer_id");

                entity.HasOne(d => d.Writer)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.WriterId)
                    .HasConstraintName("FK__posts__writer_id__1A9EF37A");
            });

            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("postImages");

                entity.Property(e => e.ImageName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("imageName");

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.HasOne(d => d.Post)
                    .WithMany()
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__postImage__post___1C873BEC");
            });

            modelBuilder.Entity<PostReport>(entity =>
            {
                entity.HasKey(e => new { e.ReporterId, e.PostId })
                    .HasName("PK_reports");

                entity.ToTable("postReports");

                entity.Property(e => e.ReporterId).HasColumnName("reporter_id");

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.Property(e => e.Reason)
                    .HasMaxLength(100)
                    .HasColumnName("reason");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__postRepor__post___226010D3");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.PostReports)
                    .HasForeignKey(d => d.ReporterId)
                    .HasConstraintName("FK__postRepor__repor__216BEC9A");
            });

            modelBuilder.Entity<ProductCat>(entity =>
            {
                entity.ToTable("productCats");

                entity.HasIndex(e => e.NameEn, "UQ__productC__F48030FAD4055A9B")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__productC__F480526DDF06487A")
                    .IsUnique();

                entity.Property(e => e.ProductCatId).HasColumnName("product_cat_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(50)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");

                entity.HasIndex(e => e.ReviewerId, "UQ__reviews__443D5A0646779C24")
                    .IsUnique();

                entity.Property(e => e.ReviewId).HasColumnName("review_id");

                entity.Property(e => e.Opinion)
                    .HasMaxLength(100)
                    .HasColumnName("opinion");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");

                entity.HasOne(d => d.Reviewer)
                    .WithOne(p => p.Review)
                    .HasForeignKey<Review>(d => d.ReviewerId)
                    .HasConstraintName("FK__reviews__reviewe__11F49EE0");
            });

            modelBuilder.Entity<SharingPatient>(entity =>
            {
                entity.HasKey(e => e.SharingId)
                    .HasName("PK__sharingP__BB366F9162B67DF5");

                entity.ToTable("sharingPatient");

                entity.Property(e => e.SharingId)
                    .ValueGeneratedNever()
                    .HasColumnName("sharing_id");

                entity.Property(e => e.CaseTypeId)
                    .HasColumnName("case_type_id")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.GovernateId).HasColumnName("governate_id");

                entity.Property(e => e.PatientPhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("patient_phone_number");

                entity.HasOne(d => d.CaseType)
                    .WithMany(p => p.SharingPatients)
                    .HasForeignKey(d => d.CaseTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__sharingPa__case___6ABAD62E");

                entity.HasOne(d => d.Governate)
                    .WithMany(p => p.SharingPatients)
                    .HasForeignKey(d => d.GovernateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__sharingPa__gover__6BAEFA67");

                entity.HasOne(d => d.Sharing)
                    .WithOne(p => p.SharingPatient)
                    .HasForeignKey<SharingPatient>(d => d.SharingId)
                    .HasConstraintName("FK__sharingPa__shari__68D28DBC");
            });

            modelBuilder.Entity<SuperAdmin>(entity =>
            {
                entity.ToTable("superAdmins");

                entity.Property(e => e.SuperAdminId)
                    .ValueGeneratedNever()
                    .HasColumnName("super_admin_id");

                entity.HasOne(d => d.SuperAdminNavigation)
                    .WithOne(p => p.SuperAdmin)
                    .HasForeignKey<SuperAdmin>(d => d.SuperAdminId)
                    .HasConstraintName("FK__superAdmi__super__190BB0C3");
            });

            modelBuilder.Entity<University>(entity =>
            {
                entity.ToTable("universities");

                entity.HasIndex(e => e.NameEn, "UQ__universi__F48030FA992D9F20")
                    .IsUnique();

                entity.HasIndex(e => e.NameAr, "UQ__universi__F480526DEB262982")
                    .IsUnique();

                entity.Property(e => e.UniversityId).HasColumnName("university_id");

                entity.Property(e => e.GovId).HasColumnName("gov_id");

                entity.Property(e => e.NameAr)
                    .HasMaxLength(100)
                    .HasColumnName("nameAr");

                entity.Property(e => e.NameEn)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("nameEn");

                entity.HasOne(d => d.Gov)
                    .WithMany(p => p.Universities)
                    .HasForeignKey(d => d.GovId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__universit__gov_i__5E54FF49");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Email, "email")
                    .IsUnique();

                entity.HasIndex(e => e.Phone, "idx_phone_notnull")
                    .IsUnique()
                    .HasFilter("([phone] IS NOT NULL)");

                entity.HasIndex(e => e.Username, "username")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Age)
                    .HasColumnName("age")
                    .HasComputedColumnSql("(datepart(year,getdate())-datepart(year,[BD]))", false);

                entity.Property(e => e.Bd)
                    .HasColumnType("date")
                    .HasColumnName("BD");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Fullname)
                    .HasMaxLength(100)
                    .HasColumnName("fullname");

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.IsVerified).HasColumnName("isVerified");

                entity.Property(e => e.Otp)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("otp")
                    .IsFixedLength();

                entity.Property(e => e.OtpExp)
                    .HasColumnType("datetime")
                    .HasColumnName("otp_exp");

                entity.Property(e => e.Password)
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasColumnName("password")
                    .IsFixedLength();

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.ProfilePicture)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("profilePicture");

                entity.Property(e => e.Salt)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("salt")
                    .IsFixedLength();

                entity.Property(e => e.Suspended).HasColumnName("suspended");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("username");

                entity.Property(e => e.VisibleContact).HasColumnName("visibleContact");

                entity.Property(e => e.VisibleMail).HasColumnName("visibleMail");

                entity.HasMany(d => d.Articles)
                    .WithMany(p => p.Likers)
                    .UsingEntity<Dictionary<string, object>>(
                        "ArticleLike",
                        l => l.HasOne<Article>().WithMany().HasForeignKey("ArticleId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__articleLi__artic__3572E547"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("LikerId").HasConstraintName("FK__articleLi__liker__347EC10E"),
                        j =>
                        {
                            j.HasKey("LikerId", "ArticleId").HasName("PK_post_likes");

                            j.ToTable("articleLikes");

                            j.IndexerProperty<int>("LikerId").HasColumnName("liker_id");

                            j.IndexerProperty<int>("ArticleId").HasColumnName("article_id");
                        });

                entity.HasMany(d => d.Blockeds)
                    .WithMany(p => p.Blockers)
                    .UsingEntity<Dictionary<string, object>>(
                        "Block",
                        l => l.HasOne<User>().WithMany().HasForeignKey("BlockedId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_blocks_users1"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("BlockerId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_blocks_users"),
                        j =>
                        {
                            j.HasKey("BlockerId", "BlockedId");

                            j.ToTable("blocks");

                            j.IndexerProperty<int>("BlockerId").HasColumnName("blocker_id");

                            j.IndexerProperty<int>("BlockedId").HasColumnName("blocked_id");
                        });

                entity.HasMany(d => d.Blockers)
                    .WithMany(p => p.Blockeds)
                    .UsingEntity<Dictionary<string, object>>(
                        "Block",
                        l => l.HasOne<User>().WithMany().HasForeignKey("BlockerId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_blocks_users"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("BlockedId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_blocks_users1"),
                        j =>
                        {
                            j.HasKey("BlockerId", "BlockedId");

                            j.ToTable("blocks");

                            j.IndexerProperty<int>("BlockerId").HasColumnName("blocker_id");

                            j.IndexerProperty<int>("BlockedId").HasColumnName("blocked_id");
                        });
            });

            modelBuilder.Entity<VerificationRequest>(entity =>
            {
                entity.HasKey(e => e.OwnerId)
                    .HasName("PK__verifica__3C4FBEE440E22A1D");

                entity.ToTable("verificationRequests");

                entity.Property(e => e.OwnerId)
                    .ValueGeneratedNever()
                    .HasColumnName("owner_id");

                entity.Property(e => e.DegreeRequested).HasColumnName("degree_requested");

                entity.Property(e => e.NatIdPhoto)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nat_id_photo");

                entity.Property(e => e.ProofOfDegreePhoto)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("proof_of_degree_photo");

                entity.Property(e => e.UniversityRequested).HasColumnName("university_requested");

                entity.HasOne(d => d.DegreeRequestedNavigation)
                    .WithMany(p => p.VerificationRequests)
                    .HasForeignKey(d => d.DegreeRequested)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__verificat__degre__049AA3C2");

                entity.HasOne(d => d.Owner)
                    .WithOne(p => p.VerificationRequest)
                    .HasForeignKey<VerificationRequest>(d => d.OwnerId)
                    .HasConstraintName("FK__verificat__owner__03A67F89");

                entity.HasOne(d => d.UniversityRequestedNavigation)
                    .WithMany(p => p.VerificationRequests)
                    .HasForeignKey(d => d.UniversityRequested)
                    .HasConstraintName("FK__verificat__unive__058EC7FB");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
