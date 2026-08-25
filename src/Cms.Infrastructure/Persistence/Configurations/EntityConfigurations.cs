using Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cms.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.LogoUrl).HasMaxLength(1000);
    }
}

public class TenantDomainConfiguration : IEntityTypeConfiguration<TenantDomain>
{
    public void Configure(EntityTypeBuilder<TenantDomain> builder)
    {
        builder.ToTable("TenantDomains");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DomainName).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.DomainName).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.Domains)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        // SQL Server refuses a table that can be reached from Tenants twice — directly, and
        // again through Sites. ClientSetNull keeps the behaviour the application relies on (the
        // domain is unbound when its website goes) without asking the database to cascade a
        // second time, which is what stopped the schema being created on SQL Server at all.
        builder.HasOne(x => x.Site)
            .WithMany(s => s.Domains)
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SiteKey).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(1000);
        builder.Property(x => x.FaviconUrl).HasMaxLength(1000);
        builder.Property(x => x.Tagline).HasMaxLength(250);
        builder.Property(x => x.PrimaryColor).HasMaxLength(20);
        builder.Property(x => x.SecondaryColor).HasMaxLength(20);
        builder.Property(x => x.HeaderImageUrl).HasMaxLength(1000);
        builder.Property(x => x.FooterText).HasMaxLength(1000);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.MapEmbedUrl).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.SiteKey }).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.Sites)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HomePageSectionConfiguration : IEntityTypeConfiguration<HomePageSection>
{
    public void Configure(EntityTypeBuilder<HomePageSection> builder)
    {
        builder.ToTable("HomePageSections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SectionKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250);
        builder.Property(x => x.SubTitle).HasMaxLength(500);
        builder.Property(x => x.ButtonText).HasMaxLength(100);
        builder.Property(x => x.ButtonLink).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.BackgroundImageUrl).HasMaxLength(1000);
        builder.Property(x => x.JsonData);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.SectionKey }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.DisplayOrder });

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Site)
            .WithMany(s => s.HomePageSections)
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Folder).HasMaxLength(100);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.StorageKey });

        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Site).WithMany().HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.TemplateKey).HasMaxLength(100);
        builder.Property(x => x.Excerpt).HasMaxLength(500);
        builder.Property(x => x.FeaturedImageUrl).HasMaxLength(1000);
        builder.Property(x => x.MetaTitle).HasMaxLength(250);
        builder.Property(x => x.MetaDescription).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.Slug }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.PageType });
        builder.HasOne(x => x.Site)
            .WithMany(s => s.Pages)
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PageTemplateConfiguration : IEntityTypeConfiguration<PageTemplate>
{
    public void Configure(EntityTypeBuilder<PageTemplate> builder)
    {
        builder.ToTable("PageTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TemplateKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DefaultSlug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.DefaultTitle).HasMaxLength(250);
        builder.HasIndex(x => x.TemplateKey).IsUnique();
        builder.HasIndex(x => x.DisplayOrder);
    }
}

public class ContactSubmissionConfiguration : IEntityTypeConfiguration<ContactSubmission>
{
    public void Configure(EntityTypeBuilder<ContactSubmission> builder)
    {
        builder.ToTable("ContactSubmissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Subject).HasMaxLength(250);
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.CreatedDate });
    }
}

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.Location }).IsUnique();
    }
}

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Target).HasMaxLength(20);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.MenuId, x.DisplayOrder });
        builder.HasOne(x => x.Menu).WithMany(x => x.Items).HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class SeoSettingConfiguration : IEntityTypeConfiguration<SeoSetting>
{
    public void Configure(EntityTypeBuilder<SeoSetting> builder)
    {
        builder.ToTable("SeoSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MetaTitle).HasMaxLength(250);
        builder.Property(x => x.MetaDescription).HasMaxLength(500);
        builder.Property(x => x.MetaKeywords).HasMaxLength(500);
        builder.Property(x => x.OgImageUrl).HasMaxLength(1000);
        builder.Property(x => x.CanonicalUrl).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.SiteId }).IsUnique();
    }
}

public class ContentEntryConfiguration : IEntityTypeConfiguration<ContentEntry>
{
    public void Configure(EntityTypeBuilder<ContentEntry> builder)
    {
        builder.ToTable("ContentEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.ContentType, x.Key }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.ContentType, x.DisplayOrder });
    }
}

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChangedProperties).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.CreatedDate });
    }
}
