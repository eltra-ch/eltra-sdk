#include "../fs_web_cam.h"

#include <iostream>

int main() {

    std::cout << "Hello World!";

   fswebcam_initialize(0,0);

   fswebcam_take_picture("image.jpg");

   fswebcam_release();

    return 0;
}
