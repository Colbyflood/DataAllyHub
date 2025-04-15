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

    public virtual DbSet<AccountType> Accounttypes { get; set; }

    public virtual DbSet<Ad> Ads { get; set; }

    public virtual DbSet<Adset> Adsets { get; set; }

    public virtual DbSet<AdCopy> Adcopies { get; set; }

    public virtual DbSet<AdMetadata> Admetadatas { get; set; }

    public virtual DbSet<AdsetAd> Adsetads { get; set; }

    public virtual DbSet<AppKpi> Appkpis { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Attribution> Attributions { get; set; }

    public virtual DbSet<BackfillFlowRequest> Backfillflowrequests { get; set; }

    public virtual DbSet<BackfillRequest> Backfillrequests { get; set; }

    public virtual DbSet<BackfillSourceHold> Backfillsourceholds { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Channelsourceflow> Channelsourceflows { get; set; }

    public virtual DbSet<ChannelType> Channeltypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyType> Companytypes { get; set; }

    public virtual DbSet<EcommerceChannel> Ecommercechannels { get; set; }

    public virtual DbSet<EcommerceKpi> Ecommercekpis { get; set; }
    
    public virtual DbSet<EcommerceMobile> Ecommercemobiles { get; set; }

    public virtual DbSet<EcommerceTotal> Ecommercetotals { get; set; }

    public virtual DbSet<EcommerceWebsite> Ecommercewebsites { get; set; }
    
    public virtual DbSet<FbBackfillRequest> Fbbackfillrequests { get; set; }

    public virtual DbSet<FbDailySchedule> Fbdailyschedules { get; set; }

    public virtual DbSet<FbRunLog> Fbrunlogs { get; set; }

    public virtual DbSet<FbRunProblem> Fbrunproblems { get; set; }

    public virtual DbSet<FbRunStaging> Fbrunstagings { get; set; }

    public virtual DbSet<FbSaveContent> Fbsavecontents { get; set; }

    public virtual DbSet<GeneralKpi> Generalkpis { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<LeadgenApplication> Leadgenapplications { get; set; }

    public virtual DbSet<LeadgenAppointment> Leadgenappointments { get; set; }

    public virtual DbSet<LeadgenContact> Leadgencontacts { get; set; }

    public virtual DbSet<LeadgenKpi> Leadgenkpis { get; set; }

    public virtual DbSet<LeadgenLead> Leadgenleads { get; set; }

    public virtual DbSet<LeadgenLocation> Leadgenlocations { get; set; }

    public virtual DbSet<LeadgenRegistration> Leadgenregistrations { get; set; }

    public virtual DbSet<LeadgenSubscription> Leadgensubscriptions { get; set; }

    public virtual DbSet<LeadgenTrial> Leadgentrials { get; set; }
    
    public virtual DbSet<Thumbnail> Thumbnails { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<TokenFbAccount> Tokenfbaccounts { get; set; }

    public virtual DbSet<VideoKpi> Videokpis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

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

        modelBuilder.Entity<AccountType>(entity =>
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
        
        modelBuilder.Entity<AdCopy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Adcopies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("adcopy_ad_fk");
        });
        
        modelBuilder.Entity<AdMetadata>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithOne(p => p.AdMetadata)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdMetadata_Ad_FK");
        });

        modelBuilder.Entity<AdsetAd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Adsetads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdsetAd_Ad_FK");

            entity.HasOne(d => d.Adset).WithMany(p => p.Adsetads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdsetAd_Adset_FK");
        });

        modelBuilder.Entity<AppKpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Appkpis).HasConstraintName("AppKpi_Ad_FK");
        });
        
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.AssetKey).HasComment("for image and video - this will be the hash from  fb\\nfor carousel it will be the ad_id\\nfor dpa or catalog it will be the product set id");
            entity.Property(e => e.AssetType).HasComment("values are IMAGE, VIDEO, DPA, CATALOG, CAROUSEL");

            entity.HasOne(d => d.Channel).WithMany(p => p.Assets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asset_channel_fk");
        });

        modelBuilder.Entity<Attribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<BackfillFlowRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channelsourceflow).WithOne(p => p.Backfillflowrequest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("channelsourceflow2_fk");
        });

        modelBuilder.Entity<BackfillRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channelsourceflow).WithOne(p => p.Backfillrequest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("channelsourceflow1_fk");
        });

        modelBuilder.Entity<BackfillSourceHold>(entity =>
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

        modelBuilder.Entity<ChannelType>(entity =>
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

        modelBuilder.Entity<CompanyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<EcommerceChannel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.EcommerceKpi).WithOne(p => p.Ecommercechannel).HasConstraintName("EcommerceChannel_ECommerce_FK");
        });

        modelBuilder.Entity<EcommerceKpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Ecommercekpis).HasConstraintName("EcommerceKpi_Ad_FK");
        });
        
        modelBuilder.Entity<EcommerceMobile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.EcommerceKpi).WithOne(p => p.Ecommercemobile).HasConstraintName("EcommerceMobile_ECommerce_FK");
        });

        modelBuilder.Entity<EcommerceTotal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.EcommerceKpi).WithOne(p => p.Ecommercetotal).HasConstraintName("EcommerceTotal_ECommerce_FK");
        });

        modelBuilder.Entity<EcommerceWebsite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.EcommerceKpi).WithOne(p => p.Ecommercewebsite).HasConstraintName("EcommerceWebsite_ECommerce_FK");
        });

        modelBuilder.Entity<FbBackfillRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithMany(p => p.Fbbackfillrequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbbackfillrequest_channel_fk");
        });
        
        modelBuilder.Entity<FbDailySchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithOne(p => p.Fbdailyschedule)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbdaily_channel_fk");
        });

        modelBuilder.Entity<FbRunLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Channel).WithMany(p => p.Fbrunlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbrunlog_channel_fk");
        });

        modelBuilder.Entity<FbRunProblem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.FbRunlog).WithMany(p => p.Fbrunproblems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbrunproblem_fbrunlog_fk");
        });

        modelBuilder.Entity<FbRunStaging>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.FbRunlog).WithMany(p => p.Fbrunstagings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fbstaging_runlog_fk");
        });

        modelBuilder.Entity<FbSaveContent>(entity =>
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

        modelBuilder.Entity<GeneralKpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.IsActive).HasDefaultValueSql("b'0'");

            entity.HasOne(d => d.Ad).WithMany(p => p.Generalkpis).HasConstraintName("GeneralKpi_Ad_FK");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<LeadgenApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgenapplication).HasConstraintName("LeadGenApplication_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenAppointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgenappointment).HasConstraintName("LeadGenAppointment_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgencontact).HasConstraintName("LeadGenContact_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenKpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Leadgenkpis).HasConstraintName("LeadGenKpi_Ad_FK");
        });

        modelBuilder.Entity<LeadgenLead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgenlead).HasConstraintName("LeadGenLead_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgenlocation).HasConstraintName("LeadGenLocation_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgenregistration).HasConstraintName("LeadGenRegistration_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgensubscription).HasConstraintName("LeadGenSubscription_LeadGen_FK");
        });

        modelBuilder.Entity<LeadgenTrial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.LeadgenKpi).WithOne(p => p.Leadgentrial).HasConstraintName("LeadGenTrial_LeadGen_FK");
        });
        
        modelBuilder.Entity<Thumbnail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
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

        modelBuilder.Entity<TokenFbAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Token).WithMany(p => p.Tokenfbaccounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TokenFBAccount_TokenId_FK");
        });

        modelBuilder.Entity<VideoKpi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Ad).WithMany(p => p.Videokpis).HasConstraintName("VideoKpi_Ad_FK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
