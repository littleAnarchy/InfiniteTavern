import { useEffect, useState, useMemo, memo } from 'react';
import Particles, { initParticlesEngine } from '@tsparticles/react';
import { loadSlim } from '@tsparticles/slim';

interface LocationParticlesProps {
  locationType: string;
}

function LocationParticles({ locationType }: LocationParticlesProps) {
  const [init, setInit] = useState(false);

  useEffect(() => {
    initParticlesEngine(async (engine) => {
      await loadSlim(engine);
    })
      .then(() => setInit(true))
      .catch(() => setInit(true));
  }, []);

  const options = useMemo(() => {
    const key = locationType.toLowerCase();
    return configs[key] || configs.tavern;
  }, [locationType]);

  if (!init) return null;

  return <Particles key={locationType} id="location-particles" options={options} />;
}

export default memo(LocationParticles);

function makeConfig(particles: Record<string, unknown>) {
  return {
    fullScreen: { enable: true, zIndex: -1 },
    fpsLimit: 60,
    detectRetina: true,
    interactivity: {
      events: {
        onClick: { enable: false },
        onHover: { enable: false },
      },
    },
    particles: {
      shape: { type: 'circle' },
      ...particles,
    },
  };
}

const configs: Record<string, ReturnType<typeof makeConfig>> = {
  // Real fire sparks — tiny bright circles that flicker fast and drift upward
  tavern: makeConfig({
    number: { value: 55 },
    color: { value: ['#ffffff', '#fff8e1', '#ffeb3b', '#ff9800', '#ff5722', '#ff3300'] },
    shape: { type: 'circle' },
    opacity: {
      value: { min: 0.05, max: 0.9 },
      animation: {
        enable: true,
        speed: 7,
        startValue: 'random',
        minimumValue: 0.05,
        destroy: 'none',
        sync: false,
      },
    },
    size: { value: { min: 0.4, max: 2 } },
    move: {
      enable: true,
      speed: { min: 0.6, max: 2.5 },
      direction: 'top',
      random: true,
      straight: false,
      outModes: { default: 'out' },
      angle: { offset: 0, value: 30 },
    },
  }),

  // Fireflies — blink from invisible to bright, float slowly and randomly
  forest: makeConfig({
    number: { value: 30 },
    color: { value: ['#69f0ae', '#b9f6ca', '#ccff90', '#f4ff81', '#e8ff80'] },
    shape: { type: 'circle' },
    opacity: {
      value: { min: 0, max: 0.8 },
      animation: { enable: true, speed: 0.3, startValue: 'random', minimumValue: 0, sync: false },
    },
    size: { value: { min: 1.5, max: 4 } },
    move: {
      enable: true,
      speed: { min: 0.1, max: 0.35 },
      direction: 'none',
      random: true,
      straight: false,
      outModes: { default: 'out' },
    },
  }),

  // Urban haze — micro dust linked by faint threads
  city: makeConfig({
    number: { value: 25 },
    color: { value: ['#cccccc', '#dddddd', '#eeeeee'] },
    links: {
      enable: true,
      distance: 130,
      color: '#cccccc',
      opacity: 0.07,
      width: 0.4,
    },
    opacity: {
      value: { min: 0.04, max: 0.18 },
      animation: { enable: true, speed: 0.25, startValue: 'random', minimumValue: 0.04, sync: false },
    },
    size: { value: { min: 0.5, max: 2 } },
    move: {
      enable: true,
      speed: { min: 0.08, max: 0.2 },
      direction: 'none',
      random: true,
      outModes: { default: 'out' },
    },
  }),

  // Stalactite drips — rapid thin drops at uneven intervals
  cave: makeConfig({
    number: { value: 35 },
    color: { value: ['#4fc3f7', '#81d4fa', '#b3e5fc', '#29b6f6', '#e1f5fe'] },
    opacity: {
      value: { min: 0.15, max: 0.7 },
      animation: { enable: true, speed: 2, startValue: 'random', minimumValue: 0.1, sync: false },
    },
    size: { value: { min: 0.5, max: 2 } },
    move: {
      enable: true,
      speed: { min: 0.8, max: 3 },
      direction: 'bottom',
      straight: true,
      random: false,
      outModes: { default: 'out' },
    },
  }),

  // Dark orbs — slow pulsing spheres connected by a shadowy web
  dungeon: makeConfig({
    number: { value: 20 },
    color: { value: ['#9c27b0', '#7b1fa2', '#e040fb', '#ab47bc', '#ce93d8'] },
    links: {
      enable: true,
      distance: 110,
      color: '#9c27b0',
      opacity: 0.15,
      width: 0.6,
    },
    opacity: {
      value: { min: 0.1, max: 0.5 },
      animation: { enable: true, speed: 0.7, startValue: 'random', minimumValue: 0.05, sync: false },
    },
    size: {
      value: { min: 3, max: 7 },
      animation: { enable: true, speed: 2, startValue: 'random', minimumValue: 2, sync: false },
    },
    move: {
      enable: true,
      speed: { min: 0.08, max: 0.25 },
      direction: 'none',
      random: true,
      outModes: { default: 'bounce' },
    },
  }),

  // Snowfall — dense flakes at varied sizes, slight horizontal drift
  mountain: makeConfig({
    number: { value: 65 },
    color: { value: ['#ffffff', '#e3f2fd', '#bbdefb', '#e8eaf6'] },
    opacity: {
      value: { min: 0.15, max: 0.75 },
      animation: { enable: true, speed: 0.5, startValue: 'random', minimumValue: 0.1, sync: false },
    },
    size: { value: { min: 1, max: 5 } },
    move: {
      enable: true,
      speed: { min: 0.3, max: 1.5 },
      direction: 'bottom',
      random: true,
      straight: false,
      outModes: { default: 'out' },
      angle: { offset: 45, value: 15 },
    },
  }),

  // Swamp gas bubbles — large, slow, wobbly ascent with opacity pulse
  swamp: makeConfig({
    number: { value: 18 },
    color: { value: ['#66bb6a', '#81c784', '#a5d6a7', '#558b2f', '#33691e'] },
    opacity: {
      value: { min: 0.06, max: 0.28 },
      animation: { enable: true, speed: 0.8, startValue: 'random', minimumValue: 0.06, sync: false },
    },
    size: {
      value: { min: 4, max: 10 },
      animation: { enable: true, speed: 2, startValue: 'random', minimumValue: 3, sync: false },
    },
    move: {
      enable: true,
      speed: { min: 0.15, max: 0.5 },
      direction: 'top',
      random: true,
      straight: false,
      outModes: { default: 'out' },
    },
  }),

  // Sandstorm — dense particles rushing in waves, layered depths
  desert: makeConfig({
    number: { value: 60 },
    color: { value: ['#d4a76a', '#c49a5c', '#e0c088', '#b8965a', '#f0d090', '#fff8e1'] },
    opacity: {
      value: { min: 0.06, max: 0.4 },
      animation: { enable: true, speed: 1.5, startValue: 'random', minimumValue: 0.06, sync: false },
    },
    size: { value: { min: 0.4, max: 3 } },
    move: {
      enable: true,
      speed: { min: 0.5, max: 3.5 },
      direction: 'right',
      random: true,
      straight: false,
      outModes: { default: 'out' },
    },
  }),

  // Haunted stone halls — near-invisible dust in a ghostly cobweb
  castle: makeConfig({
    number: { value: 22 },
    color: { value: ['#b0bec5', '#cfd8dc', '#eceff1', '#78909c'] },
    links: {
      enable: true,
      distance: 140,
      color: '#b0bec5',
      opacity: 0.08,
      width: 0.5,
    },
    opacity: {
      value: { min: 0.03, max: 0.2 },
      animation: { enable: true, speed: 0.2, startValue: 'random', minimumValue: 0.03, sync: false },
    },
    size: { value: { min: 0.5, max: 2.5 } },
    move: {
      enable: true,
      speed: { min: 0.05, max: 0.15 },
      direction: 'none',
      random: true,
      outModes: { default: 'bounce' },
    },
  }),

  // Pollen & petals — soft orbs drifting on a warm breeze
  village: makeConfig({
    number: { value: 28 },
    color: { value: ['#a5d6a7', '#fff9c4', '#ffe082', '#ffccbc', '#f8bbd0', '#dcedc8'] },
    shape: { type: 'circle' },
    opacity: {
      value: { min: 0.1, max: 0.5 },
      animation: { enable: true, speed: 0.5, startValue: 'random', minimumValue: 0.1, sync: false },
    },
    size: { value: { min: 1.5, max: 5 } },
    move: {
      enable: true,
      speed: { min: 0.1, max: 0.35 },
      direction: 'none',
      random: true,
      straight: false,
      outModes: { default: 'out' },
    },
  }),

  // Sea foam & spray — fine mist carried by salt wind
  beach: makeConfig({
    number: { value: 35 },
    color: { value: ['#80deea', '#b2ebf2', '#e0f7fa', '#ffffff', '#4dd0e1', '#80cbc4'] },
    opacity: {
      value: { min: 0.04, max: 0.35 },
      animation: { enable: true, speed: 1.5, startValue: 'random', minimumValue: 0.04, sync: false },
    },
    size: { value: { min: 0.5, max: 4 } },
    move: {
      enable: true,
      speed: { min: 0.4, max: 1.5 },
      direction: 'left',
      random: true,
      straight: false,
      outModes: { default: 'out' },
      angle: { offset: 0, value: 20 },
    },
  }),

  // Drifting ash — slow silent fall, fading in and out
  ruins: makeConfig({
    number: { value: 28 },
    color: { value: ['#9e9e9e', '#757575', '#bdbdbd', '#e0e0e0', '#424242'] },
    opacity: {
      value: { min: 0.05, max: 0.28 },
      animation: { enable: true, speed: 0.4, startValue: 'random', minimumValue: 0.05, sync: false },
    },
    size: { value: { min: 1, max: 4 } },
    move: {
      enable: true,
      speed: { min: 0.1, max: 0.5 },
      direction: 'bottom',
      random: true,
      straight: false,
      outModes: { default: 'out' },
      angle: { offset: 0, value: 40 },
    },
  }),
};
