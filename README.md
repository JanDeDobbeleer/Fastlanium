# Fastlanium

This is a rather simple tool I created using Selenium to automate the Windows Store build uploads.
I became annoyed by how much time this consumed so I fixed that.

# Usage

The only dependency is the chromedriver for Selenium to use.
You can download it [here][1].
Once downloaded, please add the location to your PATH so that Selenium can find it.

*I wanted to use PhantomJS but uploading the builds seems to fail there for now. I will update the code once that works or feel free to fire up a PR in case I take too much time ;-)*

Compile the code and use fastlanium.exe to upload your builds:

```
fastlanium <username> <password> <appId> <package>...
```

You can add as many packages needed as the last arguments, everything will be uploaded.

# Remarks

The tool will not work if you have 2 factor auth enabled on your account (obviously).

[1]:https://sites.google.com/a/chromium.org/chromedriver/downloads
