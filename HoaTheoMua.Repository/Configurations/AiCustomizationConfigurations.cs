using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class SystemPromptConfiguration : IEntityTypeConfiguration<SystemPrompt>
{
    public void Configure(EntityTypeBuilder<SystemPrompt> builder)
    {
        builder.ToTable("system_prompts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("system_prompt_types")
            .IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.HasIndex(x => x.Type).IsUnique();
    }
}

public class PlatformStandardConfiguration : IEntityTypeConfiguration<PlatformStandard>
{
    public void Configure(EntityTypeBuilder<PlatformStandard> builder)
    {
        builder.ToTable("platform_standards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Platform)
            .HasColumnName("platform")
            .HasColumnType("platform_type")
            .IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("text").IsRequired();
        builder.HasIndex(x => x.Platform);
    }
}

public class CardTemplateConfiguration : IEntityTypeConfiguration<CardTemplate>
{
    public void Configure(EntityTypeBuilder<CardTemplate> builder)
    {
        builder.ToTable("card_templates", table =>
            table.HasCheckConstraint("CK_card_templates_template_type", "template_type IN ('ai', 'handmade')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.TemplateType)
            .HasColumnName("template_type")
            .HasConversion(
                value => value == CardTemplateType.Ai ? "ai" : "handmade",
                value => value == "ai" ? CardTemplateType.Ai : CardTemplateType.Handmade)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.TemplateType, x.IsActive, x.CreatedAt })
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");
    }
}

public class BasePostConfiguration : IEntityTypeConfiguration<BasePost>
{
    public void Configure(EntityTypeBuilder<BasePost> builder)
    {
        builder.ToTable("base_posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
    }
}

public class GeneratedPostConfiguration : IEntityTypeConfiguration<GeneratedPost>
{
    public void Configure(EntityTypeBuilder<GeneratedPost> builder)
    {
        builder.ToTable("generated_posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true);
    }
}

public class GeneratedFlowerConfiguration : IEntityTypeConfiguration<GeneratedFlower>
{
    public void Configure(EntityTypeBuilder<GeneratedFlower> builder)
    {
        builder.ToTable("generated_flowers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true);
    }
}

public class GeneratedCardConfiguration : IEntityTypeConfiguration<GeneratedCard>
{
    public void Configure(EntityTypeBuilder<GeneratedCard> builder)
    {
        builder.ToTable("generated_cards", table =>
        {
            table.HasCheckConstraint("CK_generated_cards_card_type", "card_type IN ('ai', 'handmade')");
            table.HasCheckConstraint(
                "CK_generated_cards_non_negative_values",
                "size_width >= 0 AND size_height >= 0 AND size_base_price >= 0 AND size_max_words >= 0 AND word_count >= 0 AND base_price >= 0 AND extra_price >= 0 AND total_price >= 0");
            table.HasCheckConstraint(
                "CK_generated_cards_handmade",
                "card_type <> 'handmade' OR (form_type = 'calligraphy' AND attached_image_url IS NULL AND raw_image IS NULL AND base_price = 0 AND total_price = extra_price)");
            table.HasCheckConstraint(
                "CK_generated_cards_ai_images",
                "card_type <> 'ai' OR (raw_image IS NOT NULL AND image_url IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.RawImage).HasColumnName("raw_image").HasMaxLength(2048);
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.CardType)
            .HasColumnName("card_type")
            .HasConversion(
                value => value == GeneratedCardType.Ai ? "ai" : "handmade",
                value => value == "ai" ? GeneratedCardType.Ai : GeneratedCardType.Handmade)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.FormType)
            .HasColumnName("form_type")
            .HasColumnType("form_type")
            .IsRequired();
        builder.Property(x => x.SenderName).HasColumnName("sender_name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.ReceiverName).HasColumnName("receiver_name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.MessageContent).HasColumnName("message_content").HasColumnType("text").IsRequired();
        builder.Property(x => x.AttachedImageUrl).HasColumnName("attached_image_url").HasMaxLength(2048);
        builder.Property(x => x.SizeName).HasColumnName("size_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SizeWidth).HasColumnName("size_width").HasPrecision(10, 2);
        builder.Property(x => x.SizeHeight).HasColumnName("size_height").HasPrecision(10, 2);
        builder.Property(x => x.SizeBasePrice).HasColumnName("size_base_price").HasPrecision(18, 2);
        builder.Property(x => x.SizeMaxWords).HasColumnName("size_max_words");
        builder.Property(x => x.WordCount).HasColumnName("word_count");
        builder.Property(x => x.WordConfigSnapshot).HasColumnName("word_config_snapshot").HasColumnType("jsonb");
        builder.Property(x => x.BasePrice).HasColumnName("base_price").HasPrecision(18, 2);
        builder.Property(x => x.ExtraPrice).HasColumnName("extra_price").HasPrecision(18, 2);
        builder.Property(x => x.TotalPrice).HasColumnName("total_price").HasPrecision(18, 2);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(x => new { x.CardType, x.CreatedAt }).IsDescending(false, true);
    }
}

public class PostHistoryConfiguration : IEntityTypeConfiguration<PostHistory>
{
    public void Configure(EntityTypeBuilder<PostHistory> builder)
    {
        builder.ToTable("post_histories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Input).HasColumnName("input").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.BaseId).HasColumnName("base_id");
        builder.Property(x => x.OutputId).HasColumnName("output_id");
        builder.Property(x => x.SystemPromptId).HasColumnName("system_prompt_id");
        builder.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.HasOne<SystemPrompt>()
            .WithMany()
            .HasForeignKey(x => x.SystemPromptId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OutputId).IsUnique();
        builder.HasIndex(x => new { x.BaseId, x.CreatedAt }).IsDescending(false, true);
    }
}

public class MockupConfiguration : IEntityTypeConfiguration<Mockup>
{
    public void Configure(EntityTypeBuilder<Mockup> builder)
    {
        builder.ToTable("mockup");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(200);
        builder.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.IsActive, x.CreatedAt })
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}

public class ClientHistoryConfiguration : IEntityTypeConfiguration<ClientHistory>
{
    public void Configure(EntityTypeBuilder<ClientHistory> builder)
    {
        builder.ToTable("client_histories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SystemForm).HasColumnName("system_form").HasColumnType("jsonb");
        builder.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(x => x.BaseId).HasColumnName("base_id");
        builder.Property(x => x.OutputId).HasColumnName("output_id");

        // base_id/output_id are logical polymorphic references, so they intentionally have no FK.
        builder.HasIndex(x => new { x.Type, x.OutputId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Type, x.CreatedAt }).IsDescending(false, false, true);
        builder.HasIndex(x => new { x.BaseId, x.Type, x.CreatedAt }).IsDescending(false, false, true);
    }
}
