set -e
mkdir -p ./packages/
cp -r ~/.nuget/packages/libgit2sharp ./packages/
cp -r ~/.nuget/packages/libgit2sharp.nativebinaries ./packages/
docker build -t libgit2 .
