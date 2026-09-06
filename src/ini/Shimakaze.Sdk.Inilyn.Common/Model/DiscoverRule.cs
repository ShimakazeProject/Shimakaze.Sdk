namespace Shimakaze.Sdk.Inilyn.Model;

public sealed record class DiscoverRule(string? From, string? ResolveKey, string Target, string? Fallback, string? Min, string? Max);