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
            option.IsSome.Should().BeTrue(because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(option.IsSome.Should());
        }

        /// <summary>
        /// Asserts that an Option is empty (is None).
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeNone<T>(this Option<T> option, string because = "", params object[] becauseArgs)
        {
            option.IsNone.Should().BeTrue(because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(option.IsNone.Should());
        }

        /// <summary>
        /// Asserts that an Option contains a value and returns it for further assertions.
        /// </summary>
        public static T ShouldBeSomeAnd<T>(this Option<T> option, string because = "", params object[] becauseArgs)
        {
            option.IsSome.Should().BeTrue($"Expected Option to be Some{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return option.Match(value => value, () => throw new InvalidOperationException("Option was None"));
        }

        /// <summary>
        /// Asserts that an Option contains a specific value.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSome<T>(this Option<T> option, T expectedValue, string because = "", params object[] becauseArgs)
        {
            option.IsSome.Should().BeTrue($"Expected Option to be Some{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            var actualValue = option.Match(value => value, () => throw new InvalidOperationException("Option was None"));
            actualValue.Should().Be(expectedValue, because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(true.Should());
        }

        // Fin<T> Extensions

        /// <summary>
        /// Asserts that a Fin is successful.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSuccess<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            fin.IsSucc.Should().BeTrue($"Expected Fin to be Success{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(fin.IsSucc.Should());
        }

        /// <summary>
        /// Asserts that a Fin is failed.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeFail<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            fin.IsFail.Should().BeTrue($"Expected Fin to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(fin.IsFail.Should());
        }

        /// <summary>
        /// Asserts that a Fin is successful and returns the value for further assertions.
        /// </summary>
        public static T ShouldBeSuccessAnd<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            fin.IsSucc.Should().BeTrue($"Expected Fin to be Success{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return fin.Match(value => value, error => throw new InvalidOperationException($"Fin was Fail: {error}"));
        }

        /// <summary>
        /// Asserts that a Fin is successful and contains a specific value.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSuccess<T>(this Fin<T> fin, T expectedValue, string because = "", params object[] becauseArgs)
        {
            fin.IsSucc.Should().BeTrue($"Expected Fin to be Success{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            var actualValue = fin.Match(value => value, error => throw new InvalidOperationException($"Fin was Fail: {error}"));
            actualValue.Should().Be(expectedValue, because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(true.Should());
        }

        /// <summary>
        /// Asserts that a Fin is failed and returns the error for further assertions.
        /// </summary>
        public static Error ShouldBeFailAnd<T>(this Fin<T> fin, string because = "", params object[] becauseArgs)
        {
            fin.IsFail.Should().BeTrue($"Expected Fin to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return fin.Match(value => throw new InvalidOperationException($"Fin was Success: {value}"), error => error);
        }

        /// <summary>
        /// Asserts that a Fin is failed with a specific error message.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeFail<T>(this Fin<T> fin, string expectedErrorMessage, string because = "", params object[] becauseArgs)
        {
            fin.IsFail.Should().BeTrue($"Expected Fin to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            var error = fin.Match(value => throw new InvalidOperationException($"Fin was Success: {value}"), error => error);
            error.Message.Should().Be(expectedErrorMessage, because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(true.Should());
        }

        /// <summary>
        /// Asserts that a Fin is failed with an error message containing specific text.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeFailWith<T>(this Fin<T> fin, string expectedErrorSubstring, string because = "", params object[] becauseArgs)
        {
            fin.IsFail.Should().BeTrue($"Expected Fin to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            var error = fin.Match(value => throw new InvalidOperationException($"Fin was Success: {value}"), error => error);
            error.Message.Should().Contain(expectedErrorSubstring, because, becauseArgs);
            return new AndConstraint<BooleanAssertions>(true.Should());
        }

        // Validation<F, S> Extensions

        /// <summary>
        /// Asserts that a Validation is successful.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeSuccess<F, S>(this Validation<F, S> validation, string because = "", params object[] becauseArgs)
        {
            validation.IsSuccess.Should().BeTrue($"Expected Validation to be Success{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(validation.IsSuccess.Should());
        }

        /// <summary>
        /// Asserts that a Validation is failed.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeFail<F, S>(this Validation<F, S> validation, string because = "", params object[] becauseArgs)
        {
            validation.IsFail.Should().BeTrue($"Expected Validation to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(validation.IsFail.Should());
        }

        /// <summary>
        /// Asserts that a Validation is successful and returns the value for further assertions.
        /// </summary>
        public static S ShouldBeSuccessAnd<F, S>(this Validation<F, S> validation, string because = "", params object[] becauseArgs)
        {
            validation.IsSuccess.Should().BeTrue($"Expected Validation to be Success{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return validation.Match(success => success, fail => throw new InvalidOperationException($"Validation was Fail: {fail}"));
        }

        /// <summary>
        /// Asserts that a Validation is failed and returns the failure for further assertions.
        /// </summary>
        public static Seq<F> ShouldBeFailAnd<F, S>(this Validation<F, S> validation, string because = "", params object[] becauseArgs)
        {
            validation.IsFail.Should().BeTrue($"Expected Validation to be Fail{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return validation.Match(success => throw new InvalidOperationException($"Validation was Success: {success}"), fail => fail);
        }

        // Either<L, R> Extensions

        /// <summary>
        /// Asserts that an Either is Right.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeRight<L, R>(this Either<L, R> either, string because = "", params object[] becauseArgs)
        {
            either.IsRight.Should().BeTrue($"Expected Either to be Right{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(either.IsRight.Should());
        }

        /// <summary>
        /// Asserts that an Either is Left.
        /// </summary>
        public static AndConstraint<BooleanAssertions> ShouldBeLeft<L, R>(this Either<L, R> either, string because = "", params object[] becauseArgs)
        {
            either.IsLeft.Should().BeTrue($"Expected Either to be Left{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return new AndConstraint<BooleanAssertions>(either.IsLeft.Should());
        }

        /// <summary>
        /// Asserts that an Either is Right and returns the value for further assertions.
        /// </summary>
        public static R ShouldBeRightAnd<L, R>(this Either<L, R> either, string because = "", params object[] becauseArgs)
        {
            either.IsRight.Should().BeTrue($"Expected Either to be Right{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return either.Match<R>(left => throw new InvalidOperationException($"Either was Left: {left}"), right => right);
        }

        /// <summary>
        /// Asserts that an Either is Left and returns the value for further assertions.
        /// </summary>
        public static L ShouldBeLeftAnd<L, R>(this Either<L, R> either, string because = "", params object[] becauseArgs)
        {
            either.IsLeft.Should().BeTrue($"Expected Either to be Left{(string.IsNullOrEmpty(because) ? "" : $" {because}")}", becauseArgs);
            return either.Match<L>(left => left, right => throw new InvalidOperationException($"Either was Right: {right}"));
        }
    }
}