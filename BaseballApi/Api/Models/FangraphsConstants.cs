using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(Year), IsUnique = true)]
public class FangraphsConstants
{
    public int Id { get; set; }
    public int Year { get; set; }

    public decimal WOBA { get; set; }
    public decimal WOBAScale { get; set; }

    public decimal WBB { get; set; }
    public decimal WHBP { get; set; }
    public decimal W1B { get; set; }
    public decimal W2B { get; set; }
    public decimal W3B { get; set; }
    public decimal WHR { get; set; }

    public decimal RunSB { get; set; }
    public decimal RunCS { get; set; }

    [Description("Runs/PA")]
    public decimal RPA { get; set; }
    [Description("Runs/Win")]
    public decimal RW { get; set; }

    public decimal CFIP { get; set; }
}
