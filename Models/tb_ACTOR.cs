﻿using System;
using System.Collections.Generic;

namespace ProyectoVideoteca.Models;

public partial class tb_ACTOR
{
    public int ACTOR_ID { get; set; }

    public string NAME { get; set; } = null!;

    public string LAST_NAME { get; set; } = null!;

    public virtual ICollection<tb_SERIE> tb_SERIEs { get; set; } = new List<tb_SERIE>();
}
