// <copyright file="ICachingService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Livestock.Sdk.Api.FeatureFlags.Internal.Caching;

internal interface ICachingService
{
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory);
}
