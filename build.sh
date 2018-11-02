set -e
cp -r ~/.nuget/packages/libgit2sharp ./packages/
cp -r ~/.nuget/packages/libgit2sharp.nativebinaries ./packages/
docker build -t libgit2 -f libgit2/dockerfile .
