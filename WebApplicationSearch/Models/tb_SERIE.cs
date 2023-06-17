﻿using System;
using System.Collections.Generic;

namespace WebApplicationSearch.Models;

public partial class tb_SERIE
{
    public int ID { get; set; }

    public string TITLE { get; set; } = null!;

    public string? IMG { get; set; }

    public string? RELEASE_DATE { get; set; }

    public double? SCORE { get; set; }

    public int DIRECTOR_ID { get; set; }

    public int ACTOR_ID { get; set; }

    public string? GENRE { get; set; }

    public string? SYNOPSIS { get; set; }

    public string? CLASS { get; set; }

    public int QTY_SEASONS { get; set; }

    public string? VIDEO { get; set; }

    public string? DIRECTOR_NAME { get; set; }

    public string? ACTOR_NAME { get; set; }

    public virtual tb_ACTOR ACTOR { get; set; } = null!;

    public virtual tb_DIRECTOR DIRECTOR { get; set; } = null!;

    public virtual ICollection<tb_SEASON> tb_SEASONs { get; set; } = new List<tb_SEASON>();
}
