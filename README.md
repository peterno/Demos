# Demos #

## SignalR_NuGet ##

Demo from Stockholm Xamarin User Group meeting 2015-05-20.

The slides present SignalR and ModernHttpClient. Based on a slide deck from Brady Gaster from his TechEd Europe 2014 presentation.

The code demos how to use SignalR with two different servers. 

The first server acts as a chat server, i.e. resends all incoming messages. Also has the ability to start and stop a timer. The timer sends timer events from the server to all connected clients.

The second server is the server part from Demo 3 of https://github.com/bradygaster/ignite-2015-signalr. When a shape is moved, e.g. using he browser, the move data is shown in the app.

