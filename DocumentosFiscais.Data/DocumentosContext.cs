using DocumentosFiscais.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentosFiscais.Data;

public class DocumentosContext : DbContext
{
    public DocumentosContext(DbContextOptions<DocumentosContext> options) : base(options)
    {
    }
    
    public DbSet<DocumentoFiscal> DocumentosFiscais { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações específicas para DocumentoFiscal
        modelBuilder.Entity<DocumentoFiscal>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.NomeArquivo)
                  .IsRequired()
                  .HasMaxLength(255);
                  
            entity.Property(e => e.ConteudoXml)
                  .IsRequired()
                  .HasColumnType("nvarchar(max)");
                  
            entity.Property(e => e.NumeroDocumento)
                  .HasMaxLength(50);
                  
            entity.Property(e => e.HashMD5)
                  .HasMaxLength(100);
                  
            entity.Property(e => e.CnpjEmitente)
                  .HasMaxLength(14);
                  
            entity.Property(e => e.NomeEmitente)
                  .HasMaxLength(255);
                  
            entity.Property(e => e.ValorTotal)
                  .HasPrecision(18, 2);
                  
            entity.Property(e => e.DataUpload)
                  .IsRequired();
                  
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<int>();
                  
            entity.Property(e => e.Tipo)
                  .IsRequired()
                  .HasConversion<int>();
                  
            // Index para performance
            entity.HasIndex(e => e.HashMD5)
                  .IsUnique();
                  
            entity.HasIndex(e => e.DataUpload);
            
            entity.HasIndex(e => e.Tipo);
            
            entity.HasIndex(e => e.Status);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}
