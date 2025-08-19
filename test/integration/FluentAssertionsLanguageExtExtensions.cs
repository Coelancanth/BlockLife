using System;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using LanguageExt;
using LanguageExt.Common;

namespace BlockLife.test.integration
{
    /// <summary>
    /// FluentAssertions extension methods for LanguageExt types.
    /// 
    /// These extensions make it easier to assert on Option, Fin, and other
    /// functional types in integration tests while maintaining readability.
    /// </summary>
    public static class FluentAssertionsLanguageExtExtensions
    {
        // Option<T> Extensions
        
        /// <summary>
        /// Asserts that an Option contains a value (is Some).
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSome<T>(this Option<T> option, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(option.IsSome)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Option to be Some{reason}, but it was None.");
            
            return new AndConstraint<BooleanAssertions>(option.IsSome.Should());
        }
        
        /// <summary>
        /// Asserts that an Option is empty (is None).
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeNone<T>(this Option<T> option, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(option.IsNone)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Option to be None{reason}, but it was Some.");
            
            return new AndConstraint<BooleanAssertions>(option.IsNone.Should());
        }
        
        /// <summary>
        /// Asserts that an Option contains a value and returns it for further assertions.
        /// </summary>
        public static T ShouldBeSomeAnd<T>(this Option<T> option, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(option.IsSome)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Option to be Some{reason}, but it was None.");
            
            return option.IfNone(() => throw new AssertionFailedException($"Expected Some but was None. {because}"));
        }
        
        /// <summary>
        /// Asserts that an Option contains a value matching a predicate.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSomeMatching<T>(
            this Option<T> option, 
            Func<T, bool> predicate, 
            string because = "", 
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(option.IsSome)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Option to be Some{reason}, but it was None.");
            
            var matches = option.Match(
                Some: value => predicate(value),
                None: () => false
            );
            
            Execute.Assertion
                .ForCondition(matches)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Option value to match predicate{reason}, but it did not.");
            
            return new AndConstraint<BooleanAssertions>(matches.Should());
        }
        
        // Fin<T> Extensions
        
        /// <summary>
        /// Asserts that a Fin represents a successful result.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSuccess<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(fin.IsSucc)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Fin to be Success{reason}, but it was Fail: {0}.", 
                    fin.IsFail ? fin.Match(Succ: _ => "", Fail: e => e.ToString()) : "");
            
            return new AndConstraint<BooleanAssertions>(fin.IsSucc.Should());
        }
        
        /// <summary>
        /// Asserts that a Fin represents a failure.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeFail<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(fin.IsFail)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Fin to be Fail{reason}, but it was Success.");
            
            return new AndConstraint<BooleanAssertions>(fin.IsFail.Should());
        }
        
        /// <summary>
        /// Asserts that a Fin is successful and returns the value for further assertions.
        /// </summary>
        public static T ShouldBeSuccessAnd<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(fin.IsSucc)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Fin to be Success{reason}, but it was Fail: {0}.", 
                    fin.IsFail ? fin.Match(Succ: _ => "", Fail: e => e.ToString()) : "");
            
            return fin.Match(
                Succ: value => value,
                Fail: error => throw new AssertionFailedException($"Expected Success but was Fail: {error}. {because}")
            );
        }
        
        /// <summary>
        /// Asserts that a Fin is a failure and returns the error for further assertions.
        /// </summary>
        public static Error ShouldBeFailAnd<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(fin.IsFail)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Fin to be Fail{reason}, but it was Success.");
            
            return fin.Match(
                Succ: _ => throw new AssertionFailedException($"Expected Fail but was Success. {because}"),
                Fail: error => error
            );
        }
        
        /// <summary>
        /// Asserts that a Fin failure contains a specific error code.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldFailWithCode<T>(
            this Fin<T> fin, 
            string expectedCode, 
            string because = "", 
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(fin.IsFail)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Fin to be Fail{reason}, but it was Success.");
            
            var actualCode = fin.Match(
                Succ: _ => "",
                Fail: error => error.Code.ToString()
            );
            
            Execute.Assertion
                .ForCondition(actualCode == expectedCode)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected error code to be {0}{reason}, but it was {1}.", expectedCode, actualCode);
            
            return new AndConstraint<BooleanAssertions>(true.Should());
        }
        
        // Unit Extensions
        
        /// <summary>
        /// Asserts that a Unit value exists (useful for void-like operations).
        /// </summary>
        public static void ShouldBeUnit(this Unit unit, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(unit.Equals(Unit.Default))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Unit.Default{reason}.");
        }
    }
}