using System.Collections.Generic;
using SaveSyncPlugin.Core.Sync;
using Xunit;

namespace SaveSyncPlugin.Tests
{
    /// <summary>
    /// The library and the configs drift apart when metadata is refreshed
    /// ("TMNT - Mutants in Manhattan" became "Teenage Mutant Ninja Turtles: ...").
    /// The exact-match-only lookup used to return null and the save silently stopped
    /// syncing, so these tests pin both the tolerant matches and the refusals.
    /// </summary>
    public class TitleMatcherTests
    {
        private static int Match(string target, params string[] candidates)
        {
            return TitleMatcher.FindUniqueMatch(target, candidates, out _);
        }

        // ---------------- exact ----------------

        [Fact]
        public void ExactMatchWins()
        {
            Assert.Equal(1, Match("Mega Man X8 Demake", "Golden Axe Myth", "Mega Man X8 Demake"));
        }

        [Fact]
        public void ExactMatchIsCaseInsensitiveAndTrimmed()
        {
            Assert.Equal(0, TitleMatcher.FindUniqueMatch(
                "  jump force ", new List<string> { "Jump Force" }, out var exact));
            Assert.True(exact);
        }

        [Fact]
        public void ExactDuplicateTitles_KeepTheOldFirstBehaviour()
        {
            // Before TitleMatcher existed, FindGame picked the first exact match.
            Assert.Equal(0, Match("Jump Force", "Jump Force", "Jump Force"));
        }

        // ---------------- the 14 real renames ----------------

        [Theory]
        [InlineData("BigBang Beat 1st Impression", "Big Bang Beat: 1st Impression")]
        [InlineData("Crazy Taxi 3", "Crazy Taxi 3: High Roller")]
        [InlineData("Dusty Revenge CO-OP Edition", "Dusty Revenge")]
        [InlineData("Mega Man Super Fighting Robot", "Mega Man: Super Fighting Robot")]
        [InlineData("Mega Man ZX Prequel", "Mega Man ZX: The Prequel, Light & Dark")]
        [InlineData("Mega Man Rock Force", "Mega Man: Rock Force")]
        [InlineData("Mega Man Rock N Roll", "Mega Man: Rock n Roll")]
        [InlineData("Mortal Kombat Komplete Edition", "Mortal Kombat: Komplete Edition")]
        [InlineData("TMNT - Mutants in Manhattan", "Teenage Mutant Ninja Turtles: Mutants in Manhattan")]
        [InlineData("TMNT - Out of the Shadows", "Teenage Mutant Ninja Turtles: Out of the Shadows")]
        [InlineData("Touhou 07.5 - Immaterial and Missing Power", "Touhou 07.5: Immaterial and Missing Power")]
        [InlineData("Touhou 10.5 - Scarlet Weather Rhapsody", "Touhou 10.5: Scarlet Weather Rhapsody")]
        [InlineData("Touhou 12.3 - Hisoutensoku", "Touhou 12.3: Hisoutensoku")]
        [InlineData("Touhou 13.5 - Hopeless Masquerade", "Touhou 13.5: Hopeless Masquerade")]
        public void FuzzyMatchesTheRenamedLibraryTitles(string configured, string libraryTitle)
        {
            Assert.Equal(0, Match(configured, libraryTitle));
        }

        [Fact]
        public void FuzzyMatchIsNotReportedAsExact()
        {
            var index = TitleMatcher.FindUniqueMatch(
                "BigBang Beat 1st Impression",
                new[] { "Big Bang Beat: 1st Impression" },
                out var exact);

            Assert.Equal(0, index);
            Assert.False(exact);
        }

        // ---------------- refusals ----------------

        [Fact]
        public void AmbiguousFuzzyMatchIsRefused()
        {
            Assert.Equal(-1, Match(
                "Mega Man ZX",
                "Mega Man ZX: The Prequel",
                "Mega Man ZX: The Prequel, Light & Dark"));
        }

        [Fact]
        public void UnrelatedTitlesDoNotMatch()
        {
            Assert.Equal(-1, Match("Mega Man X8 Demake", "Golden Axe Myth", "The Legend of Korra"));
        }

        [Fact]
        public void SimilarButDifferentGamesDoNotMatch()
        {
            // Shares "Mega Man X" but the numbered entry is not the same game.
            Assert.Equal(-1, Match("Mega Man X4 Repliforce", "Mega Man X8 Demake", "Mega Man X Mavericks Fury"));
        }

        [Fact]
        public void EmptyTargetDoesNotMatch()
        {
            Assert.Equal(-1, Match("", "Golden Axe Myth"));
            Assert.Equal(-1, Match("   ", "Golden Axe Myth"));
            Assert.Equal(-1, Match(null!, "Golden Axe Myth"));
        }

        [Fact]
        public void EmptyOrNullCandidateListDoesNotMatch()
        {
            Assert.Equal(-1, TitleMatcher.FindUniqueMatch("Jump Force", new List<string>(), out _));
            Assert.Equal(-1, TitleMatcher.FindUniqueMatch("Jump Force", null, out _));
        }

        // ---------------- normalization ----------------

        [Fact]
        public void NormalizeExpandsKnownAbbreviation()
        {
            Assert.Equal(
                "teenage mutant ninja turtles mutants in manhattan",
                TitleMatcher.Normalize("TMNT - Mutants in Manhattan"));
        }

        [Fact]
        public void NormalizeIgnoresPunctuationAndCase()
        {
            Assert.Equal("touhou 12 3 hisoutensoku", TitleMatcher.Normalize("Touhou 12.3: HisOUTensoku"));
            Assert.Equal("mega man rock n roll", TitleMatcher.Normalize("Mega Man: Rock n Roll"));
        }
    }
}
