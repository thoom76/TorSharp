# Troubleshooting errors
## Configured user limit issue:
When you get the following error:
```
Unhandled exception. System.IO.IOException: The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached.
```
run: `export DOTNET_USE_POLLING_FILE_WATCHER=true`