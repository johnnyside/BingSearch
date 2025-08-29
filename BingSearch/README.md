# Utiliser le Dockerfile avec Display (X11)
installe VcXsrv(https://sourceforge.net/projects/vcxsrv/?utm_source=chatgpt.com)
-Lance-le en mode Multiple windows
-Décoche Native OpenGL
-Coche Disable access control
docker build -t bingsearch-x11 -f dockerfile.display .
set-variable -name DISPLAY -value "host.docker.internal:0.0"
docker run -it --rm -e DISPLAY=$DISPLAY -v /tmp/.X11-unix:/tmp/.X11-unix bingsearch-x11


docker build -t bingsearch-headless -f dockerfile.headless .
docker run -it --rm bingsearch-headless