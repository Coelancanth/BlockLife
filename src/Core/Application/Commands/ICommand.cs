using MediatR;
using System;
using LanguageExt;
using LanguageExt.Common;

namespace BlockLife.Core.Application.Commands
{
    /// <summary>
    /// A marker interface for a type that can be either a success or a failure.
    /// This is the base constraint for the MediatR logging pipeline.
    /// </summary>
    public interface ISuccessFail
    {
        void Match(Action<LanguageExt.Unit> Succ, Action<Error> Fail);
    }

    // Extend Fin<T> to implement our marker interface
    public partial class Fin<A> : ISuccessFail
    {
        public void Match(Action<LanguageExt.Unit> Succ, Action<Error> Fail) => 
            this.Match(
                Succ: a => Succ(LanguageExt.Unit.Default), 
                Fail: err => Fail(err)
            );
    }

    /// <summary>
    /// Represents a command that modifies system state and returns no data.
    /// Its handler MUST return Fin<Unit>.
    /// </summary>
    public interface ICommand : IRequest<Fin<LanguageExt.Unit>> { }

    /// <summary>
    /// Represents a command that modifies system state and returns a "Fast-Path" result.
    /// Its handler MUST return Fin<TResult>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface ICommand<TResult> : IRequest<Fin<TResult>> { }

    /// <summary>
    /// Represents a query that retrieves data and does not modify state.
    /// Its handler MUST return Fin<TResult>.
    /// </summary>
    /// <typeparam name="TResult">The type of the data being queried.</typeparam>
    public interface IQuery<TResult> : IRequest<Fin<TResult>> { }
}
