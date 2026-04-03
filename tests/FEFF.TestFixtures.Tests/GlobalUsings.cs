global using AwesomeAssertions;
global using AwesomeAssertions.Json;
global using Xunit;
global using Xunit.v3;
global using System.Net;

// global using NodaTime;
global using FEFF.Extensions;

global using static FEFF.TestFixtures.Tests.Consts;

// register the 'TestFixtures' extension
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]