﻿/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Core.Extensions
{
    public static class UserMetadataExtensions
    {
        public static IEnumerable<PropertyMetadata> GetCreateProperties(this UserMetadata userMetadata)
        {
            if (userMetadata == null) throw new ArgumentNullException("userMetadata");

            var exclude = userMetadata.CreateProperties.Select(x => x.Type);
            var additional = userMetadata.UpdateProperties.Where(x => !exclude.Contains(x.Type) && x.Required);
            return userMetadata.CreateProperties.Union(additional);
        }
    }
}