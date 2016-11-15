# spikeball
2D Platform game engine written in straight C# which features Sonic the Hedgehog style gameplay.

Spikeball is a small game engine written with the initial aim of emulating the Sonic the Hedgehog series.

<b>Features</b>
<ul>
	<li>Built in emulation the Sonic the Hedgehog movement phyiscs (as described on Sonic Retro).</li>
	<li>Completely standalone C# assembly, with no dependencies on any specific game development tool.</li>
	<li>Portable code, would be possible to port to phone apps for example.</li>
	<li>Game engine and rendering systems are completely decoupled (e.g. possible to use high res sprites).</li>
	<li>Levels/maps built using the Tiled tile editor.</li>
	<li>Collision system (traces + AABB).</li>
	<li>Basic animation system.</li>
	<li>Basic audio system.</li>
	<li>Resource management system (preload/late load).</li>
	<li>Renderer provided which uses Direct2D.</li>
</ul>

<h2>Notes</h2>
<p>
The engine is a single C# assembly however I have contained the game speciific entities in the "Gameplay" namespace as
the plan may be to eventually split the game away from the engine proper.
<br />
a
</p>