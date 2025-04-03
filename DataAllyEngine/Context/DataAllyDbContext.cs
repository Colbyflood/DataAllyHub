using System;
using System.Collections.Generic;
using DataAllyEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Context;

public partial class DataAllyDbContext : DbContext
{
    public DataAllyDbContext()
    {
    }

    public DataAllyDbContext(DbContextOptions<DataAllyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Accounttype> Accounttypes { get; set; }

    public virtual DbSet<Ad> Ads { get; set; }

    public virtual DbSet<Adset> Adsets { get; set; }

    public virtual DbSet<Adsetad> Adsetads { get; set; }

    public virtual DbSet<Appkpi> Appkpis { get; set; }

    public virtual DbSet<Attribution> Attributions { get; set; }

    public virtual DbSet<Backfillflowrequest> Backfillflowrequests { get; set; }

    public virtual DbSet<Backfillrequest> Backfillrequests { get; set; }

    public virtual DbSet<Backfillsourcehold> Backfillsourceholds { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Channelsourceflow> Channelsourceflows { get; set; }

    public virtual DbSet<Channeltype> Channeltypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Companytype> Companytypes { get; set; }

    public virtual DbSet<Ecommercechannel> Ecommercechannels { get; set; }

    public virtual DbSet<Ecommercekpi> Ecommercekpis { get; set; }

    public virtual DbSet<Ecommercetotal> Ecommercetotals { get; set; }

    public virtual DbSet<Ecommercewebsite> Ecommercewebsites { get; set; }

    public virtual DbSet<Fbdailyschedule> Fbdailyschedules { get; set; }

    public virtual DbSet<Fbrunlog> Fbrunlogs { get; set; }

    public virtual DbSet<Fbrunproblem> Fbrunproblems { get; set; }

    public virtual DbSet<Fbrunstaging> Fbrunstagings { get; set; }

    public virtual DbSet<Fbsavecontent> Fbsavecontents { get; set; }

    public virtual DbSet<Generalkpi> Generalkpis { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<Leadgenapplication> Leadgenapplications { get; set; }

    public virtual DbSet<Leadgenappointment> Leadgenappointments { get; set; }

    public virtual DbSet<Leadgencontact> Leadgencontacts { get; set; }

    public virtual DbSet<Leadgenkpi> Leadgenkpis { get; set; }

    public virtual DbSet<Leadgenlead> Leadgenleads { get; set; }

    public virtual DbSet<Leadgenlocation> Leadgenlocations { get; set; }

    public virtual DbSet<Leadgenregistration> Leadgenregistrations { get; set; }

    public virtual DbSet<Leadgensubscription> Leadgensubscriptions { get; set; }

    public virtual DbSet<Leadgentrial> Leadgentrials { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<Tokenfbaccount> Tokenfbaccounts { get; set; }

    public virtual DbSet<Videokpi> Videokpis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=da-test-db.c58gieqiiy1c.us-east-1.rds.amazonaws.com;port=3306;database=dataally;user=scaffold;password='7j8KjsP0.8hGrwnHn721LkF'");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Active).HasDefaultValueSql("b'0'");

            entity.HasOne(d => d.AccountType).WithMany(p => p.Accounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Acct_AcctType_FK");

            entity.HasOne(d => d.Company).WithMany(p => p.Accounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Acct_Company_FK");
        });

        modelBuilder.Entity<Accounttype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Ad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Adset).WithMany(p => p.Ads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ad_adset_FK");
        });

        modelBuilder.Entity<Adset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Adsets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Adset_Campaign_FK");
        });

        modelBuilder.Entity<Adsetad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Adsetads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdsetAd_Ad_FK");

            entity.HasOne(d => d.Adset).WithMany(p => p.Adsetads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdsetAd_Adset_FK");
        });

        modelBuilder.Entity<Appkpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Appkpis).HasConstraintName("AppKpi_Ad_FK");
        });

        modelBuilder.Entity<Attribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Backfillflowrequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channelsourceflow).WithOne(p => p.Backfillflowrequest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("channelsourceflow2_fk");
        });

        modelBuilder.Entity<Backfillrequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channelsourceflow).WithOne(p => p.Backfillrequest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("channelsourceflow1_fk");
        });

        modelBuilder.Entity<Backfillsourcehold>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithMany(p => p.Campaigns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Campaign_Channel_FK");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Attribution).WithMany(p => p.Channels).HasConstraintName("Channel_Attribution_FK");

            entity.HasOne(d => d.ChannelType).WithMany(p => p.Channels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Channel_ChannelType_FK");

            entity.HasOne(d => d.Client).WithMany(p => p.Channels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Channel_ClientId_FK");
        });

        modelBuilder.Entity<Channelsourceflow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithMany(p => p.Channelsourceflows)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("channel_filesequence_fk");
        });

        modelBuilder.Entity<Channeltype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Account).WithMany(p => p.Clients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Client_Account_FK");

            entity.HasOne(d => d.Industry).WithMany(p => p.Clients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Client_Industry_FK");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.Companies).HasConstraintName("companytype_id_FK");
        });

        modelBuilder.Entity<Companytype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Ecommercechannel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ecommercekpi).WithOne(p => p.Ecommercechannel).HasConstraintName("EcommerceChannel_ECommerce_FK");
        });

        modelBuilder.Entity<Ecommercekpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Ecommercekpis).HasConstraintName("EcommerceKpi_Ad_FK");
        });

        modelBuilder.Entity<Ecommercetotal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ecommercekpi).WithOne(p => p.Ecommercetotal).HasConstraintName("EcommerceTotal_ECommerce_FK");
        });

        modelBuilder.Entity<Ecommercewebsite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ecommercekpi).WithOne(p => p.Ecommercewebsite).HasConstraintName("EcommerceWebsite_ECommerce_FK");
        });

        modelBuilder.Entity<Fbdailyschedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithOne(p => p.Fbdailyschedule)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbdaily_channel_fk");
        });

        modelBuilder.Entity<Fbrunlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithMany(p => p.Fbrunlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbrunlog_channel_fk");
        });

        modelBuilder.Entity<Fbrunproblem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.FbRunlog).WithMany(p => p.Fbrunproblems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbrunproblem_fbrunlog_fk");
        });

        modelBuilder.Entity<Fbrunstaging>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.FbRunlog).WithMany(p => p.Fbrunstagings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbstaging_runlog_fk");
        });

        modelBuilder.Entity<Fbsavecontent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.AdCreativeRunlog).WithMany(p => p.FbsavecontentAdCreativeRunlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbsave_ad_creative_fk");

            entity.HasOne(d => d.AdImageRunlog).WithMany(p => p.FbsavecontentAdImageRunlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbsave_ad_image_fk");

            entity.HasOne(d => d.AdInsightRunlog).WithMany(p => p.FbsavecontentAdInsightRunlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbsave_ad_insight_fk");
        });

        modelBuilder.Entity<Generalkpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.IsActive).HasDefaultValueSql("b'0'");

            entity.HasOne(d => d.Ad).WithMany(p => p.Generalkpis).HasConstraintName("GeneralKpi_Ad_FK");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Leadgenapplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgenapplication).HasConstraintName("LeadGenApplication_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgenappointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgenappointment).HasConstraintName("LeadGenAppointment_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgencontact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgencontact).HasConstraintName("LeadGenContact_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgenkpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Leadgenkpis).HasConstraintName("LeadGenKpi_Ad_FK");
        });

        modelBuilder.Entity<Leadgenlead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgenlead).HasConstraintName("LeadGenLead_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgenlocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgenlocation).HasConstraintName("LeadGenLocation_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgenregistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgenregistration).HasConstraintName("LeadGenRegistration_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgensubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgensubscription).HasConstraintName("LeadGenSubscription_LeadGen_FK");
        });

        modelBuilder.Entity<Leadgentrial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Leadgenkpi).WithOne(p => p.Leadgentrial).HasConstraintName("LeadGenTrial_LeadGen_FK");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Enabled).HasDefaultValueSql("b'1'");

            entity.HasOne(d => d.ChannelType).WithMany(p => p.Tokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("token_channel_type_fk");

            entity.HasOne(d => d.Company).WithMany(p => p.Tokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("token_company_fk");
        });

        modelBuilder.Entity<Tokenfbaccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Token).WithMany(p => p.Tokenfbaccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TokenFBAccount_TokenId_FK");
        });

        modelBuilder.Entity<Videokpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Videokpis).HasConstraintName("VideoKpi_Ad_FK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
