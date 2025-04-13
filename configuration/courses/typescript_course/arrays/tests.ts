describe("Reverse array", () => {
  test("Test 1", () => {
    expect(reverseArray([1, 2, 3])).toEqual([3, 2, 1]);
  });
  test("Test 2", () => {
    expect(reverseArray([4, 5, 6])).toEqual([6, 5, 4]);
  });
  test("Test 3", () => {
    expect(reverseArray([7, 8, 9])).toEqual([9, 8, 7]);
  });
});
