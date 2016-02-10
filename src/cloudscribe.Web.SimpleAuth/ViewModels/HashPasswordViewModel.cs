// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:                  Joe Audette
// Created:                 2016-02-10
// Last Modified:           2016-02-10
// 

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cloudscribe.Web.SimpleAuth.ViewModels
{
    public class HashPasswordViewModel
    {
        [Required]
        [Display(Name = "Input Password")]
        public string InputPassword { get; set; } = string.Empty;

        [Display(Name = "Generated Hash")]
        public string OutputHash { get; set; } = string.Empty;
    }
}
