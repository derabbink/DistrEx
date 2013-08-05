# DistrEx

A framework for creating distributed programs - without the headaches of synchronization, timeouts, and network communication.<br/>
BTW: It stands for Distributed Execution, in case you hadn't seen that coming ;)


## What Does It Do?

DistrEx lets you write programs for distributed (multi-PC) setups easily.

Imagine a scenario where you have three machines (`pc1`, `pc2`, `pc3`) that you want to ask for the time simultaneously.
DistrEx lets you do just with a single line of code:

```c#
Coordinator3.Do(pc1.Do(GetTime), pc2.Do(GetTime), pc3.Do(GetTime)) //...
```

The result of this operation will be a `Tuple<Time, Time, Time>`, and it can be used in the next operation:

```c#
.ThenDo(pc1.Do(CompareTimes));
```

These parallel and sequential compositions can be combined/nested as required, and all execution paths are clearly visible as branches in the syntax tree.
Besides, all inputs/arguments and outputs/results are neatly typed, so if at some point your syntax tree becomes a bit overhwelming, the compiler's type checker might help you figure out what's wrong.


## How Does It Do It?

Under the hood of DistrEx you'll find a combination of [WCF][wcf], [Rx][rx] and [Reflection][reflection].

 * WCF takes care of network communication,
 * Rx elegantly cures all kinds of concurrency headaches, and
 * Reflection is used for deploying and executing code on worker machines.

Besides using these mmain components, it was built for [.NET 4.0][.net40].

## Why Does It Exist?

This project is the practical component of my MSc Thesis Project.
The intended use for DistrEx is to help design and execute automated acceptance tests for distributed (multi-pc) scenarios.

 [wcf]: http://msdn.microsoft.com/en-us/library/dd456779(v=vs.100).aspx
 [rx]: http://msdn.microsoft.com/en-us/data/gg577609.aspx
 [reflection]: http://msdn.microsoft.com/en-us/library/ms173183(v=vs.100).aspx
 [.net40]: http://www.microsoft.com/en-us/download/details.aspx?id=17851
