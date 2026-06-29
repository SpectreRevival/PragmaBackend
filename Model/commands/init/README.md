## SQL Initialization

Put SQL scripts in here and they will be executed on backend boot. This folder follows an initialization stage approach, everything in folder 0 is ran first, then folder 1 and so forth...
This is used to make sure all dependencies of a script are setup before the script is executed. Eg if Script B depends on a type in script A, script A should be in a folder with a lower number than script B.