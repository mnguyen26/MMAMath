We all know that MMA math doesn't work in real life. If Fighter A beats Fighter B, and Fighter B beats Fighter C, it doesn’t mean Fighter A will automatically beat Fighter C. Fighting ability isn't a one-dimensional trait, and matchups can be nontransitive. Styles make fights, and we've seen rock-paper-scissors scenarios like Ronda Rousey beating Misha Tate, Holly Holm defeating Ronda Rousey, and Misha Tate beating Holly Holm (Matt Hughes, Georges St-Pierre, and B.J. Penn is another example).

This project takes the idea of MMA math to its most absurd degree. Using fight results from UFC history, I've assigned fighters Elo scores (a ranking system originally developed for chess) and built a graph that connects fighters by their wins. The project lets you find the paths (if they exists) between any given fighter and the highest-rated fighters of all time, according to peak Elo scores.

Assumptions & Simplifications:

Only UFC Fights: I only tracked fights within the UFC. While it's possible to gather data from other promotions, tracing every opponent’s pre-UFC fights (and their opponents' fights, etc.) becomes impractically complex. Limiting the data to UFC fights provides a closed pool of fighters for analysis.
Simplified Elo Calculations: Elo ratings are calculated for fun and aren't meant to reflect true rankings, especially considering factors like weight class, age, or fight context.

Features:

Fighter Elo Ratings: Assigns fighters an Elo score based on their UFC fight history.
Shortest Path Algorithm: Finds the shortest path between two fighters based on wins.
Highest Elo Path: Determines the path from a fighter to the highest-rated fighters by Elo.

How to Use:

Scrape fight data: Scrape fight and event data from the web (currently pulls from Tapology).
Calculate Elo Scores: Automatically calculate Elo scores based on fight results.
MMA Math Fun: Input two fighters to see the path between them or find the fighter with the highest Elo from a given fighter.
