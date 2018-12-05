## Platform

Support .Net 4.6, Mono 4.x, .Net Standard 2.0

## Depend

Castle.Core

Newtonsoft.Json

## Install

NuGet:https://www.nuget.org/packages/Business.Core/

PM> Install-Package Business.Core

## Please refer unit test

This library has the following functions

1: Call Method with a fixed routing, including grouping mode
2: A Process for Processing Parameters
3: A standard return object wrapper

This library has the following features

1: Callbacks for method calls
2: Support object-to-parameter injection
3: A Model for Documentation
4: async Support

You can use any communication layer you like, such as ASP.Net Core MVC, WebAPI, Sockets, or even UI logic. It can dramatically improve the readability and specifications of your business logic.

Welcome Pull out to branch, but if you have a good idea, I hope it will be discussed thoroughly before you take action.

This library does not support AOT compilation mode, but can run on Xamarin.Android.