using Microsoft.EntityFrameworkCore;

using Shimakaze.Sdk.Inilyn.Data.Semantic;
using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Data;

public sealed class IniDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// 词法记号。
    /// </summary>
    public DbSet<Lexer.IniToken> Tokens => Set<Lexer.IniToken>();

    /// <summary>
    /// 分类信息。
    /// </summary>
    public DbSet<IniCategory> Categories => Set<IniCategory>();

    /// <summary>
    /// 文件信息。
    /// </summary>
    public DbSet<IniDocument> Documents => Set<IniDocument>();

    /// <summary>
    /// 段落节点。
    /// </summary>
    public DbSet<SectionNode> Sections => Set<SectionNode>();

    /// <summary>
    /// 键值对节点。
    /// </summary>
    public DbSet<KeyValuePairNode> KeyValues => Set<KeyValuePairNode>();

    /// <summary>
    /// 段落继承项。
    /// </summary>
    public DbSet<SectionInheritance> SectionInheritances => Set<SectionInheritance>();

    /// <summary>
    /// 无关记号。
    /// </summary>
    public DbSet<TriviaToken> TriviaTokens => Set<TriviaToken>();

    /// <summary>
    /// 诊断信息。
    /// </summary>
    public DbSet<IniDiagnostic> Diagnostics => Set<IniDiagnostic>();

    /// <summary>
    /// 节的语义分析结果。
    /// </summary>
    public DbSet<SectionSemanticInfo> SectionSemantics => Set<SectionSemanticInfo>();

    /// <summary>
    /// 节之间的引用关系。
    /// </summary>
    public DbSet<SectionReference> SectionReferences => Set<SectionReference>();

    /// <summary>
    /// 节的类型分配（多对多）。
    /// </summary>
    public DbSet<SectionTypeInfo> SectionTypeInfos => Set<SectionTypeInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IniCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<IniDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Path).IsUnique();
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .IsRequired();
        });

        modelBuilder.Entity<Lexer.IniToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DocumentId, e.Order });
            entity.Property(e => e.Type);
            entity.Property(e => e.Text).HasMaxLength(4096);
            entity.ComplexProperty(e => e.Position, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<SectionNode>(entity =>
        {
            entity.ToTable("Sections");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DocumentId, e.Order });
            entity.HasIndex(e => e.Name);

            entity.HasOne(e => e.Document)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentId)
                  .IsRequired();

            entity.HasMany(e => e.KeyValues)
                  .WithOne(e => e.Section)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Name).HasMaxLength(1024);

            entity.ComplexProperty(e => e.Range, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<KeyValuePairNode>(entity =>
        {
            entity.ToTable("KeyValues");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DocumentId, e.Order });
            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => e.SectionId);

            entity.Property(e => e.Key).HasMaxLength(4096);
            entity.Property(e => e.Value).HasMaxLength(65536);

            entity.ComplexProperty(e => e.Range, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<SectionInheritance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SectionNodeId, e.Order });
            entity.Property(e => e.Name).HasMaxLength(1024);
            entity.Property(e => e.Separator);
            entity.ComplexProperty(e => e.Range, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<SectionNode>()
            .HasMany(e => e.Inheritances)
            .WithOne(e => e.SectionNode)
            .HasForeignKey(e => e.SectionNodeId)
            .IsRequired();

        modelBuilder.Entity<TriviaToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DocumentId });
            entity.HasIndex(e => e.AttachedToNodeId);
            entity.Property(e => e.Kind);
            entity.Property(e => e.Text).HasMaxLength(4096);

            entity.HasOne(e => e.Document)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentId)
                  .IsRequired();

            entity.ComplexProperty(e => e.Range, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<IniDiagnostic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocumentId);
            entity.Property(e => e.Code).HasMaxLength(32);
            entity.Property(e => e.Message).HasMaxLength(4096);
            entity.Property(e => e.Severity);

            entity.HasOne(e => e.Document)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentId)
                  .IsRequired();

            entity.ComplexProperty(e => e.Range, rangeBuilder =>
            {
                rangeBuilder.ComplexProperty(r => r.Start);
                rangeBuilder.ComplexProperty(r => r.End);
            });
        });

        modelBuilder.Entity<SectionSemanticInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SectionId).IsUnique();
            entity.HasIndex(e => new { e.DocumentId, e.GroupName });
            entity.Property(e => e.GroupName).HasMaxLength(256);
            entity.Property(e => e.SectionType).HasMaxLength(256);

            entity.HasOne(e => e.Section)
                  .WithMany()
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired();
        });

        modelBuilder.Entity<SectionReference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SourceKeyValueId);
            entity.HasIndex(e => e.TargetSectionId);

            entity.HasOne(e => e.SourceKeyValue)
                  .WithMany()
                  .HasForeignKey(e => e.SourceKeyValueId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetSection)
                  .WithMany()
                  .HasForeignKey(e => e.TargetSectionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SectionTypeInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SectionId, e.TypeName }).IsUnique();
            entity.Property(e => e.TypeName).HasMaxLength(256);
        });
    }
}