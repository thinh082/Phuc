using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Phuc.Models.Entities;

public partial class PhucContext : DbContext
{
    public PhucContext()
    {
    }

    public PhucContext(DbContextOptions<PhucContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BanAn> BanAns { get; set; }

    public virtual DbSet<ChiTietDatBan> ChiTietDatBans { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DonDatBan> DonDatBans { get; set; }

    public virtual DbSet<LoaiTaiKhoan> LoaiTaiKhoans { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<ThucDon> ThucDons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Connection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BanAn__3214EC07F52AAC2E");

            entity.ToTable("BanAn");

            entity.Property(e => e.TenBan).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Trống");
            entity.Property(e => e.ViTri).HasMaxLength(255);
        });

        modelBuilder.Entity<ChiTietDatBan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC0793EC73CB");

            entity.ToTable("ChiTietDatBan");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.BanAn).WithMany(p => p.ChiTietDatBans)
                .HasForeignKey(d => d.BanAnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDa__BanAn__38996AB5");

            entity.HasOne(d => d.DonDatBan).WithMany(p => p.ChiTietDatBans)
                .HasForeignKey(d => d.DonDatBanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDa__DonDa__37A5467C");

            entity.HasOne(d => d.MonAn).WithMany(p => p.ChiTietDatBans)
                .HasForeignKey(d => d.MonAnId)
                .HasConstraintName("FK__ChiTietDa__MonAn__398D8EEE");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhGia__3214EC07FD9CB94D");

            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__TaiKhoa__4222D4EF");
        });

        modelBuilder.Entity<DonDatBan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DonDatBa__3214EC07F1B43861");

            entity.ToTable("DonDatBan");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xác nhận");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DonDatBans)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDatBan__TaiKh__33D4B598");
        });

        modelBuilder.Entity<LoaiTaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiTaiK__3214EC07E1FB615D");

            entity.ToTable("LoaiTaiKhoan");

            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC07C00830A8");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D10534C9E8938F").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.MatKhau).IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.SoLanNhapSaiMatKhau).HasDefaultValue(0);
            entity.Property(e => e.ThoiGianKhoaMatKhau).HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.LoaiTaiKhoan).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.LoaiTaiKhoanId)
                .HasConstraintName("FK__TaiKhoan__LoaiTa__2A4B4B5E");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhToa__3214EC07F5CF872B");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chưa thanh toán");

            entity.HasOne(d => d.DonDatBan).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.DonDatBanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhToan__DonDa__3D5E1FD2");
        });

        modelBuilder.Entity<ThucDon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThucDon__3214EC07B5A1BA0B");

            entity.ToTable("ThucDon");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.TenMon).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
