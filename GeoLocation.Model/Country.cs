﻿using GeoLocation.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoLocation.Model
{
    public class Country : ICountry
    {
        public Guid Id { get; set; }
        public string Abrv { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
