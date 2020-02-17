cmake_minimum_required(VERSION 2.8.12)
project (EltraDht22)

if (NOT CMAKE_BUILD_TYPE) 
	set (CMAKE_BUILD_TYPE Release)
endif ()

set(DHT22_PATH ${CMAKE_SOURCE_DIR}/)

add_library(${PROJECT_NAME} SHARED ${DHT22_PATH}/dht22.cpp 
								   ${DHT22_PATH}/sys_helper.cpp)

target_link_libraries(${PROJECT_NAME} -lwiringPi)

install(TARGETS ${PROJECT_NAME} LIBRARY DESTINATION /usr/lib)