#!/bin/sh
docker run --rm --net=host -v $PWD/data:/data loadgenerator "$@" # This will create a temporary container,
                                                                 # run the LoadGenerator and then the contain is deleted.
                                                                 # Therefore data must be save to a volume (-v).