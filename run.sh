docker run \
       -v $(pwd)/:/app \
       -v /:/volume \
       --name metodo \
       --rm \
       -ti \
       libgit2 bash

