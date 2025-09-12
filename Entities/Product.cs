using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace core8_svelte_sybase.Entities
{    
    [Table("products")]
    public class Product {

            [Key]
            public int Id { get; set; }

            [Column("category", TypeName = "VARCHAR(100)")]
            public string Category { get; set; }

            [Column("descriptions", TypeName = "VARCHAR(100)")]
            public string Descriptions { get; set; }

            [Column("qty")]
            public int Qty { get; set; }

            [Column("unit", TypeName = "VARCHAR(10)")]
            public string Unit { get; set; }

            [Column("costprice", TypeName = "DECIMAL(10,2)")]
            public decimal CostPrice { get; set; }

            [Column("sellprice", TypeName = "DECIMAL(10,2)")]
            public decimal SellPrice { get; set; }

            [Column("saleprice", TypeName = "DECIMAL(10,2)")]
            public decimal SalePrice { get; set; }

            [Column("productpicture",  TypeName = "VARCHAR(50)")]
            public string ProductPicture { get; set; }

            [Column("alertstocks")]
            public int AlertStocks { get; set; }

            [Column("criticalstocks")]
            public int CriticalStocks { get; set; }

            [Column("createdat")]
            public DateTime CreatedAt { get; set; }

            [Column("updatedat")]
            public DateTime UpdatedAt { get; set; }
    }    
}