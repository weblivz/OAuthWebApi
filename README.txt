The first release of the code. It has been tested in the lab but not in production yet, so consider is a preview release (and something to learn from).

Couple of isses I could do with some help/fresh set of eyes on (you can find these issues by searching for ### in the source):

1. In the DatabaseKeyNonceStore class if i don't check for an existing context/nonce/timestamp combination and return true if one already exists, when a duplicate is inserted, the app throws an exception. What should be the behaviour of adding an existing nonce - do i need to delete the original one?

2. If i don't call "TaskContinuationOptions.ExecuteSynchronously" to make the API handler tasks execute synchronously the app just hangs on subsequent requests. Sychronous has no issues at all but i'd like to understand what is going on.



Thanks to Andrew Arnott and David Christiansen at http://www.dotnetopenauth.net as well as http://zamd.net/ for sample code, pointers and insight.
