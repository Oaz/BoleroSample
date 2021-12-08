## BoleroSample
Variations on the [fsbolero](https://fsbolero.io/) sample client app
with [separation of concerns](https://en.wikipedia.org/wiki/Separation_of_concerns) in mind.

The [original sample](https://github.com/fsbolero/Template) is based on [elmish concepts](https://elmish.github.io/elmish/).

The code in this repo is an attempt at
- implementing elmish [patterns for parent child composition](https://elmish.github.io/elmish/parent-child.html) by splitting components into their own module
  - routing menu
  - homepage
  - counter
  - books data
  - error notification
- adding some key features
  - passing messages between components
    - for example, from books data to error notification when an http error occurs
  - injecting a view from another component
    - for example, injecting the pages into a generic routing menu


