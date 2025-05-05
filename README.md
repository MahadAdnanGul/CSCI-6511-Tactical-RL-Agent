# Tactical Reinforcement Learning Game Agent
 
This project was developed to further cement my understanding of how reinforcement learning algorithms work in practice and how they can be incorporated in creating smart game AI agents. I have created a game from scratch using Unity mostly as a graphics library rather than a game engine itself as most of my code is not really dependent on unity's own game loop but rather uses a custom game loop to simulate state-action transitions. 

## Game Description:
The game consists of a 3D maze environment with hostile turrets dynamically scanning different parts of the map. Our agent who is equipped with 1 smoke grenade has to tactically navigate the maze, avoid getting shot, utilize the smoke grenade optimally to block the unavoidable turret, keep it's health topped up and reach the goal state as fast as possible. The agent is practically blind with the exception of a sensor that can tell the agent how far it is from it's goal. During development I implemented a training system that can train using either Q-Learning, SARSA or SARSA Lambda but ultimately decided to use SARSA Lambda for the final trained agent as it was training the agent in the least amount of episodes.

## Motivation:
As a passionate game developer, I have always been very interested in properly understanding how reinforcement agents "magically" learn to solve complex games by playing the game a large number of times. This project felt like a great opportunity to implement RL algorithms and train an agent to optimally solve a hand crafted game simulation.

## How To Run:
The easiest way to run the application is via this [WebGL](https://mahad1111.itch.io/tactical-rl-agent) build hosted on itch.io. This will allow you to run the application natively on your browser. You may want to use full screen mode as some of the alert messages that appear on the top left of the screen may get cut out in the website's window.

There is also a [Windows](https://github.com/MahadAdnanGul/CSCI-6511-Tactical-RL-Agent/releases/tag/Windows) build that you can download, extract and play by running the CSCI-6511-Tactical-RL-Agent.exe file.

## How To Play:
Upon launching the game, you will be presented with this screen:
![image](https://github.com/user-attachments/assets/9b5926df-6490-4677-80ae-06660a28fd7e)
 - Play (Test Agent): See the agent playing the game over and over again using it's trained SARSA-Lambda Table. You can view the json for the table itself [Here](https://github.com/MahadAdnanGul/CSCI-6511-Tactical-RL-Agent/blob/main/Assets/Resources/sarsa_lambda_table.json)
 - Evaluate 100 Simulations: Runs a fast simulation loop where the agent performs 100 playthoughs. At each iteration you will observe the total reward at the top left of the screen. Once done, you will see a win rate being displayed.
 - Manual Mode: No AI agent. Navigate the state space manually. WASD keys to set movement direction. Space to use smoke and/or switch to idle.


## Development:
All of my scripts can be found in the Assets/Scripts folder.

At first I focused on creating a good game structure that would be well suited to run reinforcement learning algorithms. 

I started by discretizing the continous state space into 1 unit cubes (FloorTile.cs : BaseState). Each of these contained flags indicating if the state was a goal state, exposed by turret, a wall or if it contained a health item. I created the environment by placing these tiles in the map using the Unity editor.

I then created the central StateSpaceManager.cs that would dynamically create the state space by going over the tiles placed at the start of the game. The manager simply starts from the bottom left tile and works its way up, laying each state in a 2D state space array. The final state space is a 30x60 grid. This class also exposes events that other actors (agent and turret) can subscribe their own per time logic to. These events get fired after every time step tick via the ExecuteAction function.

I added various debug utilities to help troubleshoot things as I went about my development. I set up 3 debug modes: 
 - None (No visual debugging indicators).
 - Basic (color coded states based on characteristics such as walls, exposed, goal etc).
 - Extra (added exact coordinates next to each state block).

Here is an example of the extra mode showing details on each tile along with color coded states.
![Extra debug](https://github.com/user-attachments/assets/97a36688-0fb8-4a78-9df9-33320267e5b4)

I made these along with other global parameters to allow me to quickly switch between different game modes which proved very helpful for debugging and testing:
 - Learning Rate
 - Discount Factor
 - Epsilon (probability of picking a random action)
 - Lambda
 - Steps per episode
 - Total Episodes
 - Visualize training (Slows the AI simulation and enabled Unity's rendering loop so that we can see the outcomes of each time step)
 - Step Delay (Delay between time steps during visualize training mode. I also implemented interpolation in the player movement class so that the state transitions look and feel continous)
 - Debug Level
 - Training Mode (For turning off the AI and playing the game manually)
 - Ignore Save (disables saving the SARSA table for when no further training was needed)
 - Eval Mode (Runs the training for 100 episodes and displays win rate)

![image](https://github.com/user-attachments/assets/3841dfdb-1179-4fc1-a212-5a0a87660efa)

Once the basic state space was set up I created the Player.cs and PlayerMovement.cs classes. The Player class contains fields and methods that define the current state, how the agent handles things like healing, death, taking damage and if the player has a smoke available. The PlayerMovement class is responsible for selecting legal actions and the handling the outcome (state transitions) of those actions. Actions the player can take are the following:
 - Forward
 - Backward
 - Left
 - Right
 - Smoke
 - Idle

The turret class contains the logic for how the turret scans the environment at every time step. It does so by scanning checking for overlapping states within a defined radius (LOSRange) and then filtering them based on the angle (LOSAngle) between the state's position and the turret's forward direction. Then we check if there is a wall state between the turret and the state by firing a ray. Lastly we check for a smoke in a similar fasion. Finally if there are no obstructions, we mark the state as exposed. The turret also rotates by a set amount at every time step based on a defined parameter.

## Reinforcement Learning

Now that I had a strong foundation for a fully functional state space and game loop, I moved on to adding reinforcement learning to train my agent. I initially started with a Q-learning agent but eventually settled for SARSA-Lambda as it made more sense given the nature of the problem (more on this later). In the process I created a common interface (IAgentRL) that I used to implement each RL algorithm. I did this so that I can easily swap in my desired RL algorithm into the existing system whenever I needed to. Each RL Algorithm class contains a Dictionary that maps StateActionPairs to Q values (SARSA-Lambda includes an eligibility trace table in addition to this). The RL classes implement and expose the following methods:
 - SelectAction (Select random action based on epsilon probability otherwise best Q-Value).
 - UpdateRL (Update Q-Values based on formula for the specific algorithm using the current state, previous state and the reward).
 - SaveTrainingData (Write Q/SARSA table to json).
 - LoadTrainingData (Load exisiting table).

I now implemented the Trainer.cs class which is responsible for running/visualizing the training simulations as well as calculating rewards. This class takes the player and the desired RL algorithm (as an IAgentRL interface) as references. It consists of two simulation loops, regular and fast training loop. Both training loops run a simulation for each training episode which is basically a sequence of the following steps:
 - Select an action using the IAgentRL and assign that action to the player.
 - Execute the time step using stateSpaceManager.ExecuteAction().
 - Compute rewards using the reward function.
 - Update the Q values.
 - Check for terminal states.
 - Save table data periodically.

The fast loop is simply more decoupled from unity's rendering loop. This means that it simulates the training much faster but no visual feedback. I used the fast training loop for the actual training while the other is for visualization purposes.

## Reward Function and Training Process

The reward function can be found in the Trainer.cs class as ComputeRewards. Here are the reward values:
 - Fixed deduction for using Smoke: -0.2
 - Reward for a good smoke: 0.05 * number of exposed tiles covered upto a max reward of 1.0.
 - Per time step deducion for not carrying a smoke (incentivize trying to save the smoke as late as possible): -0.01.
 - Penalty for dying: -5.
 - Reward for reaching goal: 5.
 - Penalty for being on an exposed state: -0.5 (80% chance of losing health, max 3HP).
 - Penalty for collecting health item if health is full: -0.2.
 - Reward for collecting health item if health increases: 0.5.
 - Reward for getting closer to the goal than any previous state so far: 0.05.
 - Penalty for not closing any distance: -0.05.

My go to training parameters are as follows:
 - Discount Factor: 1 (Since late game moves are just as important (if not more)).
 - Learning Rate: 0.2.
 - Lambda: 0.8.
 - Epsilon: 0.1 during the first phase of training. I gradually decreased it during the second phase as I saw progress.

I started the training process using Q-learning. I went with a cirriculum style approach where I first trained the agent to solve a more simplified version of the final game. This version was essentially the same exact game but without the second turret (This one is unavoidable and needs have it's line of sight smokes to get past it). In about 5000-6000 training episodes my Q-learning agent was able to successfully avoid the first turret and reach the goal state. However, when I added the second turret the agent had the challenge of learning to use the smoke at a specific location. Initially my reward function did not have deductions for not carrying smoke and had a fixed reward for a good smoke. At this stage the agent greatly struggled to figure out when to use the smoke even with over 50000 training episodes. Infact the agent seemed to start performing worse after a certain stage and converging on throwing the smoke immediately and not going too far.

During the training process I decided to use SARSA-Lambda instead as I felt that the eligibility trace should theoretically do a better job at backpropogating the significance of throwing the late smoke to clear a path to the goal state. Once SARSA lambda was implemented, I did notice a significant speed up in training for atleast the first phase (1 turret scenario). The agent was winning most of the time in under 2000 episodes. However it was again stuck trying to figure out the correct smoke position for the final environment. I kept tweaking the reward function until I got to my current iteration. This was still not enough to get the agent to converge in meaningful time. Eventually I realized that while my reward functions do incentivize a high utility smoke as well as the notion of using the smoke as a last resort, there are far too many states for the agent to use a smoke only to learn that it was practically useless to do so. In my last iteration I adjusted my GetLegalActions Function to only make smokes a valid option if there were exposed states nearby, since that should be the only time the agent should even consider using it. Finally, the agent started showed a dramatic improvement. In about 10000 episodes, the agent was fully trained with on average 100% success rate. It learned to avoid the first turret, skip the health pack since it was unharmed and place the perfect smoke to block the second turret's vision and go straight for the goal state.
