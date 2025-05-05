# CSCI-6511-Tactical-RL-Agent
 
This project was developed to further cement my understanding of how reinforcement learning algorithms work in practice and how they can be incorporated in creating smart game AI agents. I have created a game from scratch using Unity mostly as a graphics library rather than a game engine itself as most of my code is not really dependent on unity's own game loop but rather uses a custom game loop to simulate state-action transitions. 

# Game Description:
The game consists of a 3D maze environment with hostile turrets dynamically scanning different parts of the map. Our agent who is equipped with 1 smoke grenade has to tactically navigate the maze, avoid getting shot, utilize the smoke grenade optimally to block the unavoidable turret, keep it's health topped up and reach the goal state as fast as possible. During development I implemented a training system that can train using either Q-Learning, SARSA or SARSA Lambda but ultimately decided to use SARSA Lambda for the final trained agent as it was training the agent in the least amount of episodes.

# Motivation:
As a passionate game developer, I have always been very interested in properly understanding how reinforcement agents "magically" learn to solve complex games by playing the game a large number of times. This project felt like a great opportunity to implement RL algorithms and train an agent to optimally solve a hand crafted game simulation. 

# Development:
At first I focused on creating a good game structure that would be well suited to run reinforcement learning algorithms. 
I started by discretizing the continous state space into 1 unit cubes (FloorTile.cs : BaseState). Each of these contained flags indicating if the state was a goal state, exposed by turret, a wall or if it contained a health item.
I created the environment by placing these tiles in the map using the Unity editor.
I then created a central StateSpaceManager.cs that would dynamically create the state space by going over the tiles placed at the start of the game. The manager simply starts from the bottom left tile and works its way up, laying each state in a 2D state space array. The final environment is a 30x60 grid.
I added various debug utilities to help troubleshoot things as I went about my development. I set up 3 debug modes: None (No visual debugging indicators), Basic (color coded states based on characteristics such as walls, exposed, goal etc) and Extra (added exact coordinates next to each state block).
PENDING...
//Add Player/Player movement class
//Add Turret class
//Add state transition execution and updates
//Add Trainer
//Add RL Models (AgentRL interface)
//Trainer Settings
//Add reward function
//Add training process/challenges/how I overcame them and what ended up working well/How I tried different models
//interpolation to mimic continous behavior

//How to run the application (windows or webgl build)
//App instructions (3 game modes)
//Link to sarsa lambda table

