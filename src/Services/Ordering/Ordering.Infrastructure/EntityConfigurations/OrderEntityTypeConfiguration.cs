namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        orderConfiguration.ToTable("orders", OrderingContext.DEFAULT_SCHEMA);

        orderConfiguration.HasKey(o => o.Id);

        orderConfiguration.Ignore(b => b.DomainEvents);

        orderConfiguration.Property(o => o.Id)
            .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);

        //指定 Address 属性为 Order 类的被持有实体 since EF Core 2.0
        orderConfiguration
            .OwnsOne(o => o.Address, a =>
            {
                // Explicit configuration of the shadow key property in the owned type 
                // as a workaround for a documented issue in EF Core 5: https://github.com/dotnet/efcore/issues/20740
                //EF Core 约定将为被持有的实体类型的属性数据库列命名为 EntityProperty_OwnedEntityProperty,
                //Address 的内部属性将以 Address_Street、Address_City（州、国家和邮政编码等）的名字出现在Orders 表中
                
                a.Property<int>("OrderId")
                .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);
                a.WithOwner();
            });
        //也可以用 Property().HasColumnName()这一 Fluent API 方法来重命名这些列
       // orderConfiguration.OwnsOne(p => p.Address).Property(p => p.Street).HasColumnName("ShippingStreet");

        //不使用属性 将列Column映射到字段Field，只将字段映射到表的列。常见的使用场景：内部状态的私有字段不需要被实体外部访问
        orderConfiguration
            .Property<int?>("_buyerId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("BuyerId")
            .IsRequired(false);

        orderConfiguration
            .Property<DateTime>("_orderDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)//只将字段映射到表的列
            .HasColumnName("OrderDate")
            .IsRequired();

        orderConfiguration
            .Property<int>("_orderStatusId")
            // .HasField("_orderStatusId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderStatusId")
            .IsRequired();

        orderConfiguration
            .Property<int?>("_paymentMethodId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PaymentMethodId")
            .IsRequired(false);

        orderConfiguration.Property<string>("Description").IsRequired(false);

        var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));

        // DDD Patterns comment:
        //Set as field (New since EF 1.1) to access the OrderItem collection property through its field
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        orderConfiguration.HasOne<PaymentMethod>()
            .WithMany()
            // .HasForeignKey("PaymentMethodId")
            .HasForeignKey("_paymentMethodId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        orderConfiguration.HasOne<Buyer>()
            .WithMany()
            .IsRequired(false)
            // .HasForeignKey("BuyerId");
            .HasForeignKey("_buyerId");

        orderConfiguration.HasOne(o => o.OrderStatus)
            .WithMany()
            // .HasForeignKey("OrderStatusId");
            .HasForeignKey("_orderStatusId");
    }
}
