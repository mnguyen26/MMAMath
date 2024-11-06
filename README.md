# MMA Math

A console application for extracting and transforming MMA data to support various projects.

## Overview

MMA Math is a data extraction tool designed to collect and format MMA data for use in other applications and games. This project is not intended for direct user interaction; it extracts data from both APIs and HTML web scraping to create a dataset for MMA other projects. Currently, the data is transformed and exported into a JSON format so that it can be accessed client-side without a dedicated back-end component.

So far it's been used to extract a complete history of fights in the UFC, images of all fighters, and to calculate Elo scores for all fighters. 

## Try It Out

The following apps make use of the data processed by MMA Math:

1. https://mnguyen26.github.io/mmamathapp/ - Connect two fighters through their wins to discover the "win path."
2. https://mnguyen26.github.io/mmablindrankings/ - A game to rank MMA fighters
