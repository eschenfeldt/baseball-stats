import { StatPipe } from './stat.pipe';

describe('StatPipe', () => {
  it('create an instance', () => {
    const pipe = new StatPipe();
    expect(pipe).toBeTruthy();
  });
});
